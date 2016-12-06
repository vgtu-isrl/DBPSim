using DBPSim.RuleEngine.Translator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBPSim.Simulation.Simulator.SimulationFunctions
{
    public class SimulationFunctionsProvider
    {

        public static void ProvideFunctions()
        {
            SimulationFunctionsTranslator simulationFunction = new SimulationFunctionsTranslator();
            RuleTranslator.AddAdditionalRuleTranslator(simulationFunction);
        }

    }
}
