using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBPSim.RuleEngine
{
    public class RuleEventArgs
    {

        private DateTime _eventData;

        private RuleEventArgs(bool isError, string message, Exception exception)
        {
            this._eventData = DateTime.Now;
            this.IsError = isError;
            this.Message = message;
            this.Exception = exception;
        }


        public static RuleEventArgs Load(bool isError, string message, Exception exception)
        {
            return new RuleEventArgs(isError, message, exception);
        }


        public DateTime EventDate
        {
            get
            {
                return this._eventData;
            }
        }


        public bool IsError
        {
            get;
            internal set;
        }


        public string Message
        {
            get;
            internal set;
        }


        public Exception Exception
        {
            get;
            internal set;
        }

    }
}
