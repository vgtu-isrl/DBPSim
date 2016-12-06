using System;
using System.Linq;
using System.Collections.Generic;

namespace DBPSim.RuleEngine.Memory
{

    public class WorkingMemory : Dictionary<string, object>
    {

        private static object _lockerWorkingMemory = new object();
        private static object _lockerWorkingObjects = new object();
        private Dictionary<string, object> _workingObjects = new Dictionary<string, object>();


        public WorkingMemory()
            : base(StringComparer.CurrentCultureIgnoreCase)
        {
        }


        public bool Insert(string key, object item)
        {
            bool inserted = false;
            if (!this.ContainsKey(key))
            {
                base.Add(key, item);
                inserted = true;
                RuleEngineEvents.ExecuteEvent(RuleEngineEvents.OnFactInsert, this, RuleEventArgs.Load(false, "Fact was inserted to working memory.", null));
            }
            else
            {
                this.Remove(key);
                this.Add(key, item);
            }
            return inserted;
        }


        public bool Retract(string key)
        {
            bool removed = false;
            if (this.ContainsKey(key))
            {
                lock (_lockerWorkingMemory)
                {
                    if (this.ContainsKey(key))
                    {
                        removed = this.Remove(key);
                        RuleEngineEvents.ExecuteEvent(RuleEngineEvents.OnFactRetract, this, RuleEventArgs.Load(false, "Fact was retracted from working memory.", null));
                    }
                }
            }
            return removed;
        }


        public bool Exists(string key)
        {
            if (this.ContainsKey(key))
            {
                return true;
            }
            return false;
        }


        public void InsertWorkingObject(string key, object item)
        {
            if (this.ContainsKey(key))
            {
                this[key] = item;
            }
            else
            {
                this.Add(key, item);
            }
        }


        public void RetractWorkingObject(string key)
        {
            if (this._workingObjects.ContainsKey(key))
            {
                lock (_lockerWorkingObjects)
                {
                    if (this._workingObjects.ContainsKey(key))
                    {
                        this._workingObjects.Remove(key);
                    }
                }
            }
        }


        public T ParamWorkingMemory<T>(string key)
        {
            object value;
            if (this.TryGetValue(key, out value))
            {
                return (T)value;
            }
            return default(T);
        }


        public T ParamWorkingObject<T>(string key)
        {
            object value;
            if (this._workingObjects.TryGetValue(key, out value))
            {
                return (T)value;
            }
            return default(T);
        }

    }
}
