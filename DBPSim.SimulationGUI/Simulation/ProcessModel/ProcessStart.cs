using DBPSim.Events;
using DBPSim.Model;
using DBPSim.Simulation.ProcessModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DBPSim.SimulationGUI.ProcessModel
{
    public class ProcessStart : StartAction, IProcessElement
    {

        private Event _event;
        private object _guiElement;


        public ProcessStart()
            : base("Dummy event")
        {
        }


        public ProcessStart(Event eventObj)
            : base(eventObj.Name)
        {
            this._event = eventObj;
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


        public Event AssociatedEvent
        {
            get
            {
                return this._event;
            }
        }


        private object GenerateGuiElement()
        {
            Border border = new Border();
            border.CornerRadius = new CornerRadius(50);
            border.Height = 20;
            border.Width = 20;
            border.Background = Brushes.Blue;
            return border;
        }

    }
}
