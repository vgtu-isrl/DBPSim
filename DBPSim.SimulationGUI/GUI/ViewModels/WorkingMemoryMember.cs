using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DBPSim.SimulationGUI.ViewModels
{
    public class WorkingMemoryMember
    {        

        public WorkingMemoryMember()
        {
            Properties = new List<WorkingMemoryMemberProperty>();
        }

        public WorkingMemoryMember(string Key, string DisplayString, List<WorkingMemoryMemberProperty> Properties)
        {
            this.Key = Key;
            this.DisplayString = DisplayString;
            this.Properties = Properties;

        }


        public string Key { get; set; }

        public string DisplayString { get; set; }

        public List<WorkingMemoryMemberProperty> Properties { get; set; }

        public List<WorkingMemoryMemberProperty> BeforeProperties { get; set; }

    }
}
