using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DBPSim.RuleEngine.Translator
{

    public class RuleMemoryVariablesTranslator : IRuleTranslator
    {

        private char[] _endSymbols = new char[] { '.', ',',' ', '-', '/', '*'};


        public RuleMemoryVariablesTranslator()
        {
        }


        public string Translate(RuleBase rule, string input)
        {
            // Replace all between $ and specific character with MEM expression
            foreach (char endSymbol in this._endSymbols)
            {
                string regExp = string.Format("{0}(.*?){1}", Regex.Escape("$"), Regex.Escape(endSymbol.ToString()));
                MatchCollection regExpMatches = Regex.Matches(input, regExp);
                foreach (Match regExpMatch in regExpMatches)
                {
                    input = input.Replace(regExpMatch.Value, string.Format(@"MEM(""{0}""){1}", regExpMatch.Groups[1].Value, endSymbol));
                }
            }
            // Replace other variable situations
            return input;
        }

    }
}
