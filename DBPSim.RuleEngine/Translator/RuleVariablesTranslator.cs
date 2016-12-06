using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DBPSim.RuleEngine.Translator
{
    public class RuleVariablesTranslator : IRuleTranslator
    {

        public RuleVariablesTranslator()
        {
        }


        public string Translate(RuleBase rule, string input)
        {
            //Translate new object creating, think about this
            //It may be something like %kint = new Object()
            // TODO: Don't forget implement
            string regExp = string.Format("{0}(.*?){1}", Regex.Escape("%"), Regex.Escape("= new"));
            MatchCollection regExpMatches = Regex.Matches(input, regExp);
            foreach (Match regExpMatch in regExpMatches)
            {
                //input = input.Replace(regExpMatch.Value, string.Format(@"MEM(""{0}""){1}", regExpMatch.Groups[1].Value, endSymbol));
            }
            return input;
        }

    }
}
