using DBPSim.RuleEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBPSim.Simulation.Simulator
{
    public class SimulationEventProvider
    {        

        public static void ProvideEvents()
        {
            RuleEngineEvents.OnFactInsert += OnFactInsertEvent;
        }


        private static void OnFactInsertEvent(object sender, RuleEventArgs e)
        {
            // Fire events
        }

    }
}
