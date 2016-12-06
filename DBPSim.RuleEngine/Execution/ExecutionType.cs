using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBPSim.RuleEngine.Execution
{
    public enum ExecutionType
    {
        /// <summary>
        /// Execute only condition
        /// </summary>
        Condition,
        /// <summary>
        /// Execute only body (not usable)
        /// </summary>
        Body, 
        /// <summary>
        /// Execute rule (Condition and body)
        /// </summary>
        Rule
    }
}
