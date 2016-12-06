using DBPSim.Events;
using DBPSim.RuleEngine;
using DBPSim.RuleEngine.Memory;
using DBPSim.Simulation.ProcessModel;
using DBPSim.SimulationGUI.ProcessModel;
using DBPSim.SimulationGUI.ViewModels;
using DBPSim.RuleEngine.Template;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading;
using System;
using System.Linq;


namespace DBPSim.Simulation
{
    public static class Context
    {

        private static RuleCollection _rules;
        private static WorkingMemory _workingMemory;
        private static ProcessModelInstanceCollection _processModelInstances;
        private static EventCollection _eventCollection;
        private static IDictionary<string, object> _watchPoints;
        private static long _timestamp;

        private static List<Tuple<long, RuleTemplateBase, Action>> TimeSubscribtion = new List<Tuple<long, RuleTemplateBase, Action>>();


        static Context()
        {
            Init();
        }

        public static long TimeStamp
        {
            get { return _timestamp; }
        }

        public static void ResetTime(long time)
        {
            _timestamp = time;
        }

        public static void SubscribeToTime(int timeFromNow, RuleTemplateBase rule, Action action)
        {
            TimeSubscribtion.Add(new Tuple<long,RuleTemplateBase, Action>(_timestamp+timeFromNow,rule,action));
        }
        public static void ClearTimeSubscriptions()
        {
            TimeSubscribtion.Clear();
        }

        public static bool StepInTime(RuleCollection executedRuleList)
        {
            var step = TimeSubscribtion.OrderBy(x => x.Item1).FirstOrDefault();
            if (step != null)
            {
                step.Item3();
                step.Item2.ActionTaken();
                executedRuleList.Add(step.Item2.Rule);
                //step.Item2.Duration = step.Item1;
                _timestamp = step.Item1;
                if (step.Item2.State == ExecutionStateAcitivity.ExecutionState.finished)
                {
                    SimulationLogViewModel.WriteLineWithDate("{0} {1} subtask finished.", _timestamp, step.Item2.Rule.Title);  //step.Item2.Rule.Title + " finished.");
                }
                TimeSubscribtion.Remove(step);
                return true;
            }
            return false;
        }
        

        public static RuleCollection Rules
        {
            get
            {
                return _rules;
            }
            set
            {
                _rules = value;
            }
        }


        public static WorkingMemory Memory
        {
            get
            {
                return _workingMemory;
            }
            set
            {
                _workingMemory = value;
            }
        }


        public static ProcessModelInstanceCollection ProcessModelInstances
        {
            get
            {
                return _processModelInstances;
            }
            set
            {
                _processModelInstances = value;
            }
        }


        public static EventCollection Events
        {
            get
            {
                return _eventCollection;
            }
            set
            {
                _eventCollection = value;
            }
        }


        public static IDictionary<string, object> WatchingPoints
        {
            get
            {
                return _watchPoints;
            }
            set
            {
                _watchPoints = value;
                WatchPoints.Init((ExpandoObject)value);
            }
        }


        public static void Init()
        {
            // Init context
            _rules = new RuleCollection();
            _workingMemory = new WorkingMemory();
            _processModelInstances = new ProcessModelInstanceCollection();
            _eventCollection = new EventCollection();
            WatchPoints.Init();
            _watchPoints = (IDictionary<string, object>)WatchPoints.Instance;

            // Init viewmodels
            EventsViewModel.Init();
            ProcessModelInstancesViewModel.Init();
            SimulationLogViewModel.Refresh();
            SimulationWatchPointViewModel.Init();
        }

    }
}
