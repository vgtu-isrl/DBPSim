using DBPSim.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBPSim.SimulationGUI.ViewModels
{
    public class EventViewModelMember
    {

        public EventViewModelMember()
        {
            this.Properties = new List<EventViewModelPropertyMember>();
        }


        public Event Event { get; set; }

        public string Name { get; set; }
        public DateTime? EventDate { get; set; }
        public List<EventViewModelPropertyMember> Properties { get; set; }

        public bool Processed { get; set; }
        public bool Fired { get; set; }

    }
}
