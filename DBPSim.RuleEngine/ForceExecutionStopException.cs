using System;

namespace DBPSim.RuleEngine
{

    public class ForceExecutionStopException : Exception
    {

        public ForceExecutionStopException()
            : base("Execution was force stopped by user")
        {
        }


        public ForceExecutionStopException(string message)
            : base(message)
        {
        }

    }

}
