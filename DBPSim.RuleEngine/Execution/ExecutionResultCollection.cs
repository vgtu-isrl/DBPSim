using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBPSim.RuleEngine.Execution
{
    public class ExecutionResultCollection : List<ExecutionResult>
    {

        private int _capacity;


        public ExecutionResultCollection(int capacity) : base(capacity)
        {
            if (capacity <= 0)
            {
                throw new Exception("Capacity must be greater than 0.");
            }
            this._capacity = capacity;
        }

        
        public new void Add(ExecutionResult item)
        {
            base.Add(item);
            lock (this)
            {
                // Remove last element
                if (this.Count >= this._capacity)
                {
                    this.RemoveAt(0);
                }
            }
        }

    }
}
