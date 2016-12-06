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
    /// Interaction logic for FullRule.xaml
    /// </summary>
    public partial class FullRule : Window
    {

        public FullRule()
        {
            InitializeComponent();
        }


        public FullRule(string translatedRule)
        {
            InitializeComponent();
            string[] translatedRuleLines = translatedRule.Split(new string[] { "\n" }, StringSplitOptions.None);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < translatedRuleLines.Length; i++)
            {
                sb.AppendFormat("{0}. {1}", i + 1, translatedRuleLines[i]);
            }

            this.TextBlockTranslatedRule.Text = sb.ToString();
        }

    }
}
