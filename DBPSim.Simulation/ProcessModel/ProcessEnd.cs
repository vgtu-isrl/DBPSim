using DBPSim.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DBPSim.Simulation.ProcessModel
{
    public class ProcessEnd : EndAction, IProcessElement
    {

        private object _guiElement;


        public ProcessEnd()
            : base("End")
        {
        }


        public object GUIElement
        {
            get
            {
                if (this._guiElement == null)
                {
                    this._guiElement = this.GenerateGuiElement();
                }
                return this._guiElement;
            }
        }


        private object GenerateGuiElement()
        {
            Border border = new Border();
            border.CornerRadius = new CornerRadius(50);
            border.Height = 20;
            border.Width = 20;
            border.Background = Brushes.Red;
            return border;
        }

    }
}
