using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBPSim.RuleEngine.Translator
{
    public class RuleEndTranslator : IRuleTranslator
    {

        public RuleEndTranslator()
        {
        }


        public string Translate(RuleBase rule, string input)
        {
            return input.Replace("(END)", " throw new EndExecutionException() ");
        }

    }
}
