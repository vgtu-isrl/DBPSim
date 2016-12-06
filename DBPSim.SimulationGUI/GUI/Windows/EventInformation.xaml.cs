using DBPSim.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DBPSim.SimulationGUI.Windows
{
    /// <summary>
    /// Interaction logic for EventInformation.xaml
    /// </summary>
    public partial class EventInformation : Window
    {

        public EventInformation()
        {
            InitializeComponent();
        }


        public EventInformation(Event evnt)
        {
            InitializeComponent();

            this.LabelEventName.Content = evnt.Name.ToString();
            if (evnt.EventDate != null)
            {
                this.LabelEventDate.Content = evnt.EventDate.Value.ToString();
            }

            foreach (KeyValuePair<string, object> keyValuePair in evnt.EventExpando)
            {
                Run run1 = new Run() { Text = keyValuePair.Key, FontWeight = FontWeights.Bold };
                Run run2 = new Run() { Text = " = " + keyValuePair.Value.ToString() + "\n" };
                this.TextBlockEventAttributes.Inlines.Add(run1);
                this.TextBlockEventAttributes.Inlines.Add(run2);
            }
        }


    }
}
