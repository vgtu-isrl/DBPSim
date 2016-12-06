using DBPSim.Events;
using System;

namespace DBPSim.Simulation.ProcessModel
{
    public class ProcessModelInstance
    {

        private Event _event;
        private DateTime _created;
        private bool _finished;

        private int _number;

        public int Number
        {
            get { return _number; }
        }

        public ProcessModelInstance(int number)
        {
            this._created = DateTime.Now;
            this._finished = false;

            this._number = number;
        }


        public ProcessModelInstance(Event evnt, int number)
            : this(number)
        {
            this._event = evnt;
        }


        public ProcessAction FirstProcessAction { get; internal set; }
        public ProcessAction CurrentProcessAction { get; internal set; }


        public DateTime Created
        {
            get { return this._created; }
        }


        public bool Finished
        {
            get { return this._finished; }
            set { this._finished = value; }
        }


        public string DisplayTitle
        {
            get
            {
                return this.ToString();
            }
        }


        public Event Event
        {
            get { return this._event; }
        }


        public void SetCurrentProcessAction(ProcessAction action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            // Set first
            if (this.FirstProcessAction == null)
            {
                this.FirstProcessAction = action;
            }

            // Set add action for current
            if (this.CurrentProcessAction != null)
            {
                ProcessAction processAction = this.CurrentProcessAction.ProcessActions.Find(item => item.Equals(action));
                if (processAction == null)
                {
                    this.CurrentProcessAction.ProcessActions.Add(action);
                }
                else
                {
                    action = processAction;
                }
            }

            // Set actions
            action.PreviousAction = this.CurrentProcessAction;
            this.CurrentProcessAction = action;

            // Active UI
            action.SetActive();
            if (action.PreviousAction != null)
            {
                action.PreviousAction.SetToPrevious();
            }
        }


        public ProcessAction BackProcessAction()
        {
            if (this.CurrentProcessAction == null)
            {
                return null;
            }
            if (this.CurrentProcessAction.PreviousAction == null)
            {
                return null;
            }

            // Active UI
            this.CurrentProcessAction.SetNonActive();
            this.CurrentProcessAction.PreviousAction.SetActive();

            // Current action
            this.CurrentProcessAction = this.CurrentProcessAction.PreviousAction;

            return this.CurrentProcessAction;
        }


        public override string ToString()
        {
            if (this._finished)
            {
                return this._number + ". " + this.Created.ToString() + " Finished";
            }
            return this._number + ". " + this.Created.ToString();
        }

    }
}
