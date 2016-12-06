using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBPSim.RuleEngine.Execution
{
    public class ExecutionResult
    {
        
        private CompilerErrorCollection _compilerErros = null;
        private Exception _outsideException;
        private RuleExecutionResult _ruleExecutionResult = null;
        private DateTime _timeExecuted;


        public bool SuccessfullyExecuted
        {
            get
            {
                return (this._ruleExecutionResult != null && (this._compilerErros == null || this._compilerErros.Count == 0));
            }
        }


        public DateTime ExecutionDate
        {
            get
            {
                return this._timeExecuted;
            }
        }


        public CompilerErrorCollection Errors
        {
            get
            {
                return this._compilerErros;
            }
            set
            {
                this._compilerErros = value;
            }
        }


        public Exception ExternalException
        {
            get
            {
                return this._outsideException;
            }
            set
            {
                this._outsideException = value;
            }
        }


        public RuleExecutionResult Result
        {
            get
            {
                return this._ruleExecutionResult;
            }
            set
            {
                this._ruleExecutionResult = value;
            }
        }


        public ExecutionResult()
        {
            this._timeExecuted = DateTime.Now;
        }

    }
}
