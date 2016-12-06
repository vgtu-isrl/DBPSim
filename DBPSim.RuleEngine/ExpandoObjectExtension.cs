using DBPSim.RuleEngine.Memory;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBPSim.RuleEngine
{
    public static class ExpandoObjectExtension
    {

        public static bool Exists(this ExpandoObject obj, string key)
        {
            return ((IDictionary<string, object>)WatchPoints.Instance).ContainsKey(key);
        }

    }
}
