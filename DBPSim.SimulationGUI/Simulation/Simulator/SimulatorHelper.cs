using DBPSim.Events;
using DBPSim.RuleEngine;
using DBPSim.RuleEngine.Memory;
using DBPSim.Simulation.ProcessModel;
using DBPSim.SimulationGUI.ProcessModel;
using DBPSim.SimulationGUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace DBPSim.Simulation.Simulator
{
    public class SimulatorHelper
    {

        public  class SimulationRuntime
        {
            public RulesEngine Engine = new RulesEngine();
            public SimulatorStatus Status = SimulatorStatus.Unknown;
            public ProcessModelInstance ProcessInstance;
            private long? _simulationTime;
            public long? SimulationTime
            {
                get
                {
                    return _simulationTime;
                }
                set
                {
                    _simulationTime = value;
                }
            }

            public SimulationRuntime()
            {
                Engine.Subscribe = new Action<int, DBPSim.RuleEngine.Template.RuleTemplateBase, Action>
                    ((int timefromnow, DBPSim.RuleEngine.Template.RuleTemplateBase template, Action act) =>
                    {
                        Context.SubscribeToTime(timefromnow, template, act);
                    });
                Engine.StepInTime = new Func<RuleCollection,bool>((rules)=> {
                    var result = Context.StepInTime(rules);
                    SimulationTime = (int?)Context.TimeStamp;
                    return result;
                });
            }
            
        }

        private static SimulationRuntime _modeledInstance = null;

        public static RulesEngine GetEngine(int instanceNumber)
        {
            return simulationRuntimes.Where(x => x.ProcessInstance.Number==instanceNumber).FirstOrDefault().Engine;
        }
        public static SimulationRuntime GetSimulationInstance(int instanceNumber)
        {
            return simulationRuntimes.Where(x => x.ProcessInstance.Number == instanceNumber).FirstOrDefault();
        }

        public static long? GetSimulationTime(int instanceNumber)
        {
            return simulationRuntimes.Where(x => x.ProcessInstance.Number == instanceNumber).FirstOrDefault().SimulationTime;
        }
        public static SimulatorStatus GetSimulationStatus(int instanceNumber)
        {
            return simulationRuntimes.Where(x => x.ProcessInstance.Number == instanceNumber).FirstOrDefault().Status;
        }
        public static bool IsInited()
        {
            return simulationRuntimes.All(x => x.Status == SimulatorStatus.Unknown ||
                x.Status == SimulatorStatus.Inited ||
                x.Status == SimulatorStatus.Finished);
        }
        public static bool IsStarted()
        {
            return !simulationRuntimes.All(x => x.Status == SimulatorStatus.Finished || x.Status == SimulatorStatus.Unknown);
        }
        public static SimulationRuntime GetCurrentlyModeledInstance()
        {
            return _modeledInstance;
        }


        private static List<SimulationRuntime> simulationRuntimes = new List<SimulationRuntime>();

       //private static List<RulesEngine> _engine = new List<RulesEngine>();
       //private static SimulatorStatus _status = SimulatorStatus.Unknown;
       //private static ProcessModelInstance _currentProcessModelInstance;

        // Simulation process instance number
        private static int _simulationProcessInstance;

        // Simulation time
        //private static DateTime? _simulationTime = null;
              

        //public static RulesEngine Engine
        //{
        //    get
        //    {
        //        return _engine;
        //    }
        //}


        //public static DateTime? SimulationTime
        //{
        //    get { return _simulationTime; }
        //    set { _simulationTime = value; }
        //}


       // public static SimulatorStatus Status
       // {
       //     get
       //     {
       //         return _status;
       //     }
       //}


        public static void InitSimulator(bool parallel=false)
        {


            SimulationRuntime runtime = new SimulationRuntime();

            Event evnt = null;
            if (runtime.SimulationTime != null)
            {
                evnt = Context.Events.GetProcessingEvent(new DateTime(1970,1,1).AddMinutes(runtime.SimulationTime.Value));
            }
            else
            {
                evnt = Context.Events.GetProcessingEvent();
            }

            if (evnt == null)
            {
                runtime.ProcessInstance = null;
                return;
            }



            runtime.Engine.InitEngine();
            //Context.Memory = new WorkingMemory();
            runtime.Engine.WorkingMemory = Context.Memory;
            runtime.Engine.Rules = Context.Rules;


            runtime.Engine.RuleChanged = new Action(() =>
            {
                Context.Rules = runtime.Engine.Rules;
                RulesViewModel.Init();
            });

            runtime.ProcessInstance = new ProcessModelInstance(evnt, _simulationProcessInstance++);
            Context.Memory.InsertWorkingObject(evnt.Name, evnt.EventExpando);
            Context.ProcessModelInstances.Add(runtime.ProcessInstance);

            ProcessModelInstancesViewModel.Init();

            runtime.Status = SimulatorStatus.Inited;

            if (!parallel)
            {
                simulationRuntimes = new List<SimulationRuntime>();
            }
            simulationRuntimes.Add(runtime);

            if (_modeledInstance == null || _modeledInstance.Status == SimulatorStatus.Error || _modeledInstance.Status == SimulatorStatus.Finished || _modeledInstance.Status == SimulatorStatus.Unknown)
            {
                _modeledInstance = runtime;
            }

        }


        public static void RestartSimulation()
        {
            simulationRuntimes = new List<SimulationRuntime>();

            //_status = SimulatorStatus.Inited;
            _simulationProcessInstance = 0;

            SimulationLogViewModel.Refresh();
            Context.Memory = new WorkingMemory();
            Context.ProcessModelInstances = new ProcessModelInstanceCollection();
            Context.Events.Restart();
            Context.WatchingPoints.Clear();
            Context.Memory.Clear();
            Context.ClearTimeSubscriptions();
        }


        public static void NextStep(out bool finished)
        {
            finished = false;
            bool errorOccured = false;
            bool ruleExecutedSuccessfully = false;

            simulationRuntimes.Where(x => x.Status == SimulatorStatus.Finished).ToList().ForEach(x => simulationRuntimes.Remove(x));            


            foreach (var simulationRuntime in simulationRuntimes)
            {
                List<RuleBase> executedRules = null;
                try
                {
                    ruleExecutedSuccessfully = simulationRuntime.Engine.FireAllRulesOneStep(out executedRules);
                }
                catch (Exception ex)
                {
                    SimulationLogViewModel.WriteLineWithDate("Error: " + ex.Message);
                    errorOccured = true;
                }

                if (errorOccured)
                {
                    SimulationLogViewModel.WriteLineWithDate("Error occured. Please go to activity tab and investigate.");
                    finished = true;
                }
                else if (ruleExecutedSuccessfully)
                {
                    simulationRuntime.Status = SimulatorStatus.StepByStepSimulation;
                    executedRules.ForEach(executedRule =>
                    {
                        SimulationLogViewModel.WriteLineWithDate("{2} Activity Id: {0} Title: {1}", executedRule.Id, executedRule.Title, simulationRuntime.SimulationTime);
                        ProcessAction processAction = new ProcessAction(executedRule);

                        if (simulationRuntime.ProcessInstance.Event.EventDate != null)
                        {
                            if (executedRule.Duration != null)
                            {

                                int time = (int)((simulationRuntime.ProcessInstance.Event.EventDate.Value.AddMinutes(executedRule.Duration.Value) - new DateTime(1970, 1, 1)).TotalMinutes + 0.5);
                                Context.ResetTime(time);
                                simulationRuntime.SimulationTime = time;
                            }
                            else
                            {
                                int time = (int)((simulationRuntime.ProcessInstance.Event.EventDate.Value - new DateTime(1970, 1, 1)).TotalMinutes + 0.5);
                                simulationRuntime.SimulationTime = time;
                                Context.ResetTime(time);
                            }
                        }
                        processAction.SimulationTime = simulationRuntime.SimulationTime;

                        simulationRuntime.ProcessInstance.SetCurrentProcessAction(processAction);
                        simulationRuntime.ProcessInstance.Event.SetEventStateToFired();

                    });                    
                    
                }
                else
                {
                    // Finish.

                    simulationRuntime.Status = SimulatorStatus.Finished;
                    SimulationLogViewModel.WriteLineWithDate("Process instance simulation finished.");
                    simulationRuntime.ProcessInstance.Finished = true;

                    InitSimulator();
                    if (simulationRuntime.ProcessInstance == null)
                    {
                        finished = true;
                    }
                }
            }

            

            // Refresh UI
            WorkingMemoryViewModel.Init();
            SimulationTimeViewModel.Init();
            ProcessModelInstancesViewModel.Init();
            SimulationWatchPointViewModel.Init();
        }


        public static void PreviousStep(int instanceNumber)
        {
            var runtime = simulationRuntimes.Where(x => x.ProcessInstance.Number == instanceNumber).FirstOrDefault();



            ProcessAction processAction = runtime.ProcessInstance.BackProcessAction();
            if (processAction != null)
            {
                runtime.Engine.WorkingMemory = processAction.WorkingMemoryInstance;
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
