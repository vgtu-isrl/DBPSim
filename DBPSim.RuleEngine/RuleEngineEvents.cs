using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBPSim.RuleEngine
{
    public static class RuleEngineEvents
    {

        // Any event
        public static EventHandler<RuleEventArgs> OnAnyEvent = null;

        //General Events
        public static EventHandler<RuleEventArgs> OnRulesChanged = null;

        // Rule events
        public static EventHandler<RuleEventArgs> OnConditionException = null;
        public static EventHandler<RuleEventArgs> OnBodyException = null;

        // Execution events
        public static EventHandler<RuleEventArgs> OnExecutionStarted = null;
        public static EventHandler<RuleEventArgs> OnExecutionEnded = null;

        // Rule changing event
        public static EventHandler<RuleEventArgs> OnRuleChanged = null;        

        // Fact events
        public static EventHandler<RuleEventArgs> OnNewFactCreate = null;        
        public static EventHandler<RuleEventArgs> OnFactInsert = null;        
        public static EventHandler<RuleEventArgs> OnFactRetract = null;

        public static EventHandler<RuleEventArgs> OnResourceAllocated = null;
        public static EventHandler<RuleEventArgs> OnResourceRetracted = null;

        // Debug events

        public static void ExecuteEvent(EventHandler<RuleEventArgs> evt, object sender, RuleEventArgs e)
        {
            if (sender == null)
            {
                throw new ArgumentNullException("sender");
            }
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            if (OnAnyEvent != null)
            {
                OnAnyEvent(sender, e);
            }
            // Execute specified event
            if (evt != null)
            {
                evt(sender, e);
            }
        }

    }
}
