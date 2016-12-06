using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBPSim.RuleEngine.Memory
{
    public class MemoryEntryPoint : Dictionary<string, object>
    {

        private string _title;


        public MemoryEntryPoint(string title) : base(StringComparer.OrdinalIgnoreCase)
        {
            this._title = title;
        }

    }
}
