using DBPSim.RuleEngine;
using DBPSim.RuleEngine.Translator;
using DBPSim.SimulationGUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DBPSim.Simulation.Simulator.SimulationFunctions
{
    class SimulationFunctionsTranslator : IRuleTranslator
    {

        private string _regExpText = null;
        private string _functionNamespaceTextString = null;

        private string _regExpFunction = null;
        private string _functionNamespaceFunctionString = null;

        public SimulationFunctionsTranslator()
        {
            this._regExpText = string.Format("{0}(.*?){1}", Regex.Escape(@"Log("""), Regex.Escape(@""")"));
            this._functionNamespaceTextString = typeof(SimulationLogViewModel).Namespace + "." + typeof(SimulationLogViewModel).Name + @".WriteLineWithDate(""{0}"") ";

            this._regExpFunction = string.Format("{0}(.*?){1}", Regex.Escape("Log("), Regex.Escape(@")"));
            this._functionNamespaceFunctionString = typeof(SimulationLogViewModel).Namespace + "." + typeof(SimulationLogViewModel).Name + @".WriteObject({0}) ";
        }


        public string Translate(RuleBase rule, string input)
        {
            MatchCollection regExpMatchesText = Regex.Matches(input, this._regExpText);
            foreach (Match regExpMatch in regExpMatchesText)
            {
                input = input.Replace(regExpMatch.Value, string.Format(this._functionNamespaceTextString, regExpMatch.Groups[1].Value));
            }

            MatchCollection regExpMatchesFunction = Regex.Matches(input, this._regExpFunction);
            foreach (Match regExpMatch in regExpMatchesFunction)
            {
                input = input.Replace(regExpMatch.Value, string.Format(this._functionNamespaceFunctionString, regExpMatch.Groups[1].Value));
            }

            return input;
        }
    }
}
