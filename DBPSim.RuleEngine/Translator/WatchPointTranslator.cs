using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DBPSim.RuleEngine.Translator
{
    public class WatchPointTranslator : IRuleTranslator
    {

        private char[] _watchPointEndSymbols = new char[] { '.', ',', ' ', '-', '/', '*' };


        public WatchPointTranslator()
        {
        }


        public string Translate(RuleBase rule, string input)
        {
            input = input.Replace("WatchPoints.Exists", "CType(WatchPoints.Instance, System.Dynamic.ExpandoObject).Exists");

            // Replace all between # and specific character with WATCHPOINT method expression
            foreach (char endSymbol in this._watchPointEndSymbols)
            {
                string regExp = string.Format("{0}(.*?){1}", Regex.Escape("#"), Regex.Escape(endSymbol.ToString()));
                MatchCollection regExpMatches = Regex.Matches(input, regExp);
                foreach (Match regExpMatch in regExpMatches)
                {
                    input = input.Replace(regExpMatch.Value, string.Format(@"WatchPoints.Instance.{0}{1}", regExpMatch.Groups[1].Value, endSymbol));
                }
            }

            // Replace other variable situations
            return input;
        }

    }
}
