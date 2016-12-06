using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBPSim.RuleEngine
{
    public class EndExecutionException : Exception
    {

        public EndExecutionException()
            : base("Execution ended")
        {
        }

    }
}
