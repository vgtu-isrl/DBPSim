using DBPSim.Events;
using DBPSim.RuleEngine;
using DBPSim.RuleEngine.Memory;
using DBPSim.Simulation.ProcessModel;
using DBPSim.SimulationGUI.ProcessModel;
using DBPSim.SimulationGUI.ViewModels;
using System.Collections.Generic;
using System.Dynamic;


namespace DBPSim.Simulation
{
    public static class Context
    {

        private static RuleCollection _rules;
        private static WorkingMemory _workingMemory;
        private static ProcessModelInstanceCollection _processModelInstances;
        private static EventCollection _eventCollection;
        private static IDictionary<string, object> _watchPoints;


        static Context()
        {
            Init();
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
