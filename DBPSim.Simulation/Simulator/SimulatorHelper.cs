using DBPSim.Events;
using DBPSim.RuleEngine;
using DBPSim.RuleEngine.Memory;
using DBPSim.Simulation.ProcessModel;
using DBPSim.SimulationGUI.ProcessModel;
using DBPSim.SimulationGUI.ViewModels;
using System;
using System.Collections.Generic;

namespace DBPSim.Simulation.Simulator
{
    public class SimulatorHelper
    {

        private static RulesEngine _engine = new RulesEngine();
        private static SimulatorStatus _status = SimulatorStatus.Unknown;
        private static ProcessModelInstance _currentProcessModelInstance;

        // Simulation process instance number
        private static int _simulationProcessInstance;

        // Simulation time
        private static DateTime? _simulationTime = null;


        public static RulesEngine Engine
        {
            get
            {
                return _engine;
            }
        }


        public static DateTime? SimulationTime
        {
            get { return _simulationTime; }
            set { _simulationTime = value; }
        }


        public static SimulatorStatus Status
        {
            get
            {
                return _status;
            }
       }


        public static void InitSimulator()
        {
            Event evnt = null;
            if (_simulationTime != null)
            {
                evnt = Context.Events.GetProcessingEvent(_simulationTime.Value);
            }
            else
            {
                evnt = Context.Events.GetProcessingEvent();
            }            

            if (evnt == null)
            {
                _currentProcessModelInstance = null;
                return;
            }

            _engine = new RulesEngine();
            //Context.Memory = new WorkingMemory();
            _engine.WorkingMemory = Context.Memory;
            _engine.Rules = Context.Rules;

            _currentProcessModelInstance = new ProcessModelInstance(evnt, _simulationProcessInstance++);
            Context.Memory.InsertWorkingObject(evnt.Name, evnt.EventExpando);
            Context.ProcessModelInstances.Add(_currentProcessModelInstance);            

            ProcessModelInstancesViewModel.Init();

            _status = SimulatorStatus.Inited;
        }


        public static void RestartSimulation()
        {
            _status = SimulatorStatus.Inited;
            _simulationProcessInstance = 0;

            SimulationLogViewModel.Refresh();
            Context.Memory = new WorkingMemory();
            Context.ProcessModelInstances = new ProcessModelInstanceCollection();
            Context.Events.Restart();
            Context.WatchingPoints.Clear();
        }


        public static void NextStep(out bool finished)
        {
            finished = false;
            bool errorOccured = false;
            bool ruleExecutedSuccessfully = false;

            RuleBase executedRule = null;
            try
            {
                ruleExecutedSuccessfully = Engine.FireAllRulesOneStep(out executedRule);
            }
            catch (Exception ex)
            {
                SimulationLogViewModel.WriteLineWithDate("Error: " + ex.Message);
                errorOccured = true;
            }

            if (errorOccured)
            {
                SimulationLogViewModel.WriteLineWithDate("Error occured. Please go to activity tab ant investigate.");
                finished = true;
            }
            else if (ruleExecutedSuccessfully)
            {
                _status = SimulatorStatus.StepByStepSimulation;
                SimulationLogViewModel.WriteLineWithDate("Activity Id: {0} Title: {1}", executedRule.Id, executedRule.Title);

                ProcessAction processAction = new ProcessAction(executedRule);

                if (_currentProcessModelInstance.Event.EventDate != null)
                {
                    if (executedRule.Duration != null)
                    {
                        _simulationTime = _currentProcessModelInstance.Event.EventDate.Value.AddMinutes(executedRule.Duration.Value);
                    }
                    else
                    {
                        _simulationTime = _currentProcessModelInstance.Event.EventDate.Value;
                    }
                }
                processAction.SimulationTime = _simulationTime;

                _currentProcessModelInstance.SetCurrentProcessAction(processAction);
                _currentProcessModelInstance.Event.SetEventStateToFired();
            }
            else
            {
                // Finish.

                _status = SimulatorStatus.Finished;
                SimulationLogViewModel.WriteLineWithDate("Process instance simulation finished.");
                _currentProcessModelInstance.Finished = true;

                InitSimulator();
                if (_currentProcessModelInstance == null)
                {
                    finished = true;
                }
            }

            // Refresh UI
            WorkingMemoryViewModel.Init();
            SimulationTimeViewModel.Init();
            ProcessModelInstancesViewModel.Init();
            SimulationWatchPointViewModel.Init();
        }


        public static void PreviousStep()
        {
            ProcessAction processAction = _currentProcessModelInstance.BackProcessAction();
            if (processAction != null)
            {
                Engine.WorkingMemory = processAction.WorkingMemoryInstance;
                Context.Memory = processAction.WorkingMemoryInstance;
                Context.WatchingPoints = (IDictionary<string, object>)processAction.WatchPointInstance.ExpandoObject;

                SimulationLogViewModel.WriteLineWithDate("Backing one step previous...");
            }
            else
            {
                SimulationLogViewModel.WriteLineWithDate("Cannot find previous process action.");
            }
        }

    }
}
