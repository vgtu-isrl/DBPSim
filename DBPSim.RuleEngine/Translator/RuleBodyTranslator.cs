using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBPSim.RuleEngine.Translator
{

    public class RuleBodyTranslator : IRuleTranslator
    {

        public RuleBodyTranslator()
        {            
        }


        public string Translate(RuleBase rule, string input)
        {
            return input.Replace("{[BODY]}", rule.Body);
        }

    }

}
