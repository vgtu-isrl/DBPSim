using System;
using System.Collections.Generic;

namespace DBPSim.RuleEngine.Execution
{
    public class RuleExecutionResult
    {

        private RuleBase _rule;
        private Exception _internalBodyException;
        private Exception _internalConditionException;
        private Dictionary<string, object> _resultParameters = new Dictionary<string, object>();


        public Dictionary<string, object> ResultParameters
        {
            get
            {
                return this._resultParameters;
            }
        }


        public Exception ConditionException
        {
            get { return this._internalConditionException; }
            set { this._internalConditionException = value; }
        }


        public Exception BodyException
        {
            get { return this._internalBodyException; }
            set { this._internalBodyException = value; }
        }


        public RuleExecutionResult(RuleBase rule)
        {
            this._rule = rule;
        }


        // Push some result
        public void Push(string parameterName, object variable)
        {
            this._resultParameters.Add(parameterName, variable);
        }


        public T GetParameter<T>(string parameterId)
        {
            object parameter;
            if (this._resultParameters.TryGetValue(parameterId, out parameter))
            {
                return (T)parameter;
            }
            return default(T);
        }

    }
}
