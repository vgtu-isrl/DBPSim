using DBPSim.RuleEngine;
using DBPSim.RuleEngine.Validation;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBPSim.Events
{
    public class EventSyntaxValidator
    {

        public static bool Validate(string configuration, out CompilerErrorCollection errors)
        {
            RuleNonConditional rule = new RuleNonConditional();
            rule.Id = "ValidationRule";
            rule.Title = "ValidationRule";
            rule.Body = configuration;

            EventRuleTranslator ruleTranslator = new EventRuleTranslator();
            ruleTranslator.Translate(rule, null);

            return Validator.ValidateRuleSyntax(rule, out errors);
        }


        public static string GetFullRule(string configuration)
        {
            RuleNonConditional rule = new RuleNonConditional();
            rule.Id = "ValidationRule";
            rule.Title = "ValidationRule";
            rule.Body = configuration;

            EventRuleTranslator ruleTranslator = new EventRuleTranslator();
            ruleTranslator.Translate(rule, null);

            return rule.FullRule;
        }

    }
}
