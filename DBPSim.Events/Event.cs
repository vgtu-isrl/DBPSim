using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace DBPSim.Events
{
    public class Event
    {

        private string _name;
        private string _configuration;

        private ExpandoObject _eventExpando;
        private DateTime? _eventDate;

        private bool _processed;
        private bool _fired;

        
        public Event(string name, string configuration, DateTime? date, ExpandoObject expandoObject)
        {
            this._name = name;
            this._configuration = configuration;
            this._eventDate = date;
            this._eventExpando = expandoObject;
            this._processed = false;
            this._fired = false;
        }


        public string Name
        {
            get { return this._name; }
        }


        public string Configuration
        {
            get { return this._configuration; }
        }


        public ExpandoObject EventExpando
        {
            get { return this._eventExpando; }
        }


        public DateTime? EventDate
        {
            get { return this._eventDate; }
        }


        public bool Processed
        {
            get { return this._processed; }
        }


        public bool Fired
        {
            get { return this._fired; }
        }


        public void SetEventStateToProcessed()
        {
            this._processed = true;
        }


        public void SetEventStateToNotProcessed()
        {
            this._processed = false;
            this._fired = false;
        }


        public void SetEventStateToFired()
        {
            this._fired = true;
        }


        public override string ToString()
        {
            return this.Name + " " + this.Processed.ToString();
        }

    }
}
