using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DBPSim.SimulationGUI.ViewModels
{
    public class WorkingMemoryMemberProperty
    {

        private System.Windows.Media.Brush _foregroundColor = System.Windows.Media.Brushes.Black;
        private System.Windows.FontWeight _currentFontWeight = System.Windows.FontWeights.Normal;

        public FontWeight CurrentFontWeight
        {
            get
            {
                return _currentFontWeight;
            }
            set
            {
                _currentFontWeight = value;
            }
        }


        public string PropertyName { get; set; }


        public object PropertyValue { get; set; }


        public System.Windows.Media.Brush ForegroundColor
        {
            get { return this._foregroundColor; }
            set
            {
                this._foregroundColor = value;
            }
        }

    }
}
