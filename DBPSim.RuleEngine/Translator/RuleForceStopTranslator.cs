 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBPSim.RuleEngine.Translator
{
    public class RuleForceStopTranslator : IRuleTranslator
    {
                
        public RuleForceStopTranslator()
        {                        
        }


        public string Translate(RuleBase rule, string input)
        {
            return input.Replace("(STOP)", " throw new ForceExecutionStopException ");
        }

    }
}
