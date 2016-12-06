using DBPSim.RuleEngine;
using DBPSim.RuleEngine.Translator;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace DBPSim.Events
{
    class EventRuleTranslator : IRuleTranslator
    {

        string _regExp = null;


        public EventRuleTranslator()
        {
            this._regExp = string.Format("{0}(.*?){1}", Regex.Escape("Random("), Regex.Escape(@")"));
        }


        public string Translate(RuleBase rule, string input)
        {
            MatchCollection regExpMatchesFunction = Regex.Matches(rule.Body, this._regExp);
            foreach (Match regExpMatch in regExpMatchesFunction)
            {
                rule.Body = rule.Body.Replace(regExpMatch.Value, string.Format("random.Next({0})", regExpMatch.Groups[1].Value));
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(@"CREATE(""event"")");
            sb.AppendLine("Dim random As New Random");

            string[] inputLines = rule.Body.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string inputLine in inputLines)
            {
                sb.AppendLine(@"MEM(""event"")." + inputLine.TrimStart());
            }

            sb.AppendLine(@"PUSH(""event"", MEM(""event""))");

            rule.Body = sb.ToString();

            return input;
        }

    }
}
