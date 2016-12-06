
namespace DBPSim.RuleEngine
{

    public class RuleConditional : RuleBase
    {

        public RuleConditional()
            : base()
        {
        }


        public RuleConditional(string ruleId, string ruleTitle, string ruleCondition, string ruleBody)
            : base(ruleId, 0, ruleTitle, ruleCondition, ruleBody)
        {
        }

        public RuleConditional(string ruleId, int? priority, string ruleTitle, string ruleCondition, string ruleBody)
            : base(ruleId, priority, ruleTitle, ruleCondition, ruleBody)
        {
        }

    }
}
