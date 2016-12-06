using DBPSim.Model;
using DBPSim.RuleEngine;
using DBPSim.RuleEngine.Memory;
using DBPSim.SimulationGUI.HelperClasses;
using DBPSim.SimulationGUI.ModelDrawer;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DBPSim.Simulation.ProcessModel
{
    public class ProcessAction : ModelAction, IProcessElement
    {

        private RuleBase _rule;
        private DateTime _dateCreated;

        private int _actionStartMinute;
        private long? _simulationTime;

        private WorkingMemory _workingMemoryInstance;
        private WatchPointInstance _watchPointInstance;

        private ProcessActionCollection _processActions;

        private Brush _elementColor;


        public ProcessAction(RuleBase rule)
            : base(rule.Title)
        {
            this._rule = rule;
            this._dateCreated = DateTime.Now;
            this._workingMemoryInstance = ObjectCloner.CloneWorkingMemory(rule.RulesEngine.WorkingMemory);
            this._watchPointInstance = WatchPointInstance.CloneFromRuleEngine();

            this._processActions = new ProcessActionCollection();
        }



        public object GUIElement
        {
            get
            {
                return SimulationModelInstanceElementHelper.GetActionElement(this._rule.Title);                
                //return null;
            }
        }


        public WorkingMemory WorkingMemoryInstance
        {
            get
            {
                return this._workingMemoryInstance;
            }
        }


        public WatchPointInstance WatchPointInstance
        {
            get { return this._watchPointInstance; }
        }


        public ProcessAction PreviousAction { get; set; }
        public ProcessActionCollection ProcessActions
        {
            get
            {
                return this._processActions;
            }
        }


        public DateTime Created
        {
            get { return this._dateCreated; }
        }


        public int ActionStartMinute
        {
            get { return this._actionStartMinute; }
            set { this._actionStartMinute = value; }
        }
        public long? SimulationTime
        {
            get { return this._simulationTime; }
            set { this._simulationTime = value; }
        }


        public Brush Color
        {
            get { return this._elementColor; }
        }


        public void SetActive()
        {
            this._elementColor = Brushes.LightGreen;
        }


        public void SetToPrevious()
        {
            this._elementColor = Brushes.Yellow;
        }


        public void SetNonActive()
        {
            this._elementColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F2BCAC"));
        }


        public override bool Equals(object obj)
        {
            ProcessAction processAction = (ProcessAction)obj;
            bool isEqual = true;

            if (processAction._rule.Id.Equals(this._rule.Id, System.StringComparison.InvariantCultureIgnoreCase))
            {
                // Compare working memory
                IDictionary<string, object> currentMemory = (IDictionary<string, object>)this._workingMemoryInstance;
                IDictionary<string, object> actionMemory = (IDictionary<string, object>)processAction._workingMemoryInstance;

                if (currentMemory.Keys.Count != actionMemory.Keys.Count)
                {
                    isEqual = false;
                }

                foreach (KeyValuePair<string, object> current in currentMemory)
                {
                    if (actionMemory.ContainsKey(current.Key))
                    {
                        if (!(actionMemory[current.Key] == current.Value))
                        {
                            isEqual = false;
                            break;
                        }
                    }
                    else
                    {
                        isEqual = false;
                        break;
                    }
                }
            }
            else
            {
                isEqual = false;
            }

            return isEqual;
        }


        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }
}
