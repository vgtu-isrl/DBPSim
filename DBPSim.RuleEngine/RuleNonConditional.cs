using DBPSim.RuleEngine.Translator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBPSim.RuleEngine
{
    public class RuleNonConditional : RuleBase
    {


        public RuleNonConditional()
            : base()
        {
        }


        public RuleNonConditional(string ruleId, int priority, string ruleTitle, string ruleBody)
            : base(ruleId, priority, ruleTitle, null, ruleBody)
        {
        }


        public override string Condition
        {
            get
            {
                return "TRUE";

            }
            set
            {
                throw new InvalidOperationException("Rule non conditional condition is always TRUE");
            }    
        }


        public override string FullRule
        {
            get
            {
                return RuleTranslator.Translate(this);
            }
        }

    }
}
