using DBPSim.SimulationGUI.HelperClasses;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBPSim.Simulation.ProcessModel
{
    public class WatchPointInstance
    {

        public WatchPointInstance()
        {
        }

        public ExpandoObject ExpandoObject { get; set; }


        public static WatchPointInstance CloneFromRuleEngine()
        {
            WatchPointInstance watchPointInstance = new WatchPointInstance();

            ExpandoObject obj = ObjectCloner.CloneExpandoObject((ExpandoObject)Context.WatchingPoints);

            watchPointInstance.ExpandoObject = obj;

            return watchPointInstance;
        }

    }
}
