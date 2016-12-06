using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBPSim.RuleEngine.Translator
{
    class FactExistsTranslator : IRuleTranslator
    {

        public FactExistsTranslator()
        {
        }


        public string Translate(RuleBase rule, string input)
        {
            input = input.Replace("Fact.Exists", "Me.Memory.Exists");

            // Replace other variable situations
            return input;
        }

    }
}
