using DBPSim.RuleEngine.Execution;
using DBPSim.RuleEngine.Memory;
using System;

namespace DBPSim.RuleEngine.Template
{
    public class RuleTemplate : RuleTemplateBase
    {

        public RuleTemplate(RulesEngine rulesEngine, WorkingMemory memory, RuleBase rule)
            : base(rulesEngine,memory, rule)
        {
        }
        public bool ExecuteCondition()
        {
            RuleBase rule = this._rule;
            try
            {
                // RULE CONDITION GOES HERE
            }
            catch (Exception ex)
            {                
                this._ruleResult.ConditionException = ex;
            }
            return false;
        }


        public RuleExecutionResult ExecuteBody()
        {
            RuleBase rule = this._rule;
            try
            {
                
                // RULE BODY
            }
            catch (Exception ex)
            {
                this._ruleResult.BodyException = ex;
            }
            if (this._eventFiringEnabled)
            {
                //this._allRules.FireAllRules();
            }
            return this._ruleResult;
        }


        public override RuleExecutionResult Execute()
        {
            if (this.ExecuteCondition())
            {
                return this.ExecuteBody();
            }
            return this._ruleResult;
        }

    }
}
