using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBPSim.RuleEngine.Translator
{

    public class RuleConditionTranslator : IRuleTranslator
    {

        public RuleConditionTranslator()
        {
        }


        public string Translate(RuleBase rule, string input)
        {
            return input.Replace("{[CONDITION]}", rule.Condition);
        }

    }

}
