using DBPSim.Simulation.Simulator;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBPSim.SimulationGUI.ViewModels
{
    public class SimulationTimeViewModel : INotifyPropertyChanged
    {

        private static SimulationTimeViewModel _viewModelInstance;

        public SimulationTimeViewModel()
        {
            _viewModelInstance = this;
        }


        public static void Init()
        {
            if (_viewModelInstance != null)
            {
                _viewModelInstance.InitInternal();
            }
        }


        public long? SimulationTime { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;


        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        private void PopulateMembers()
        {
            this.SimulationTime = SimulatorHelper.GetCurrentlyModeledInstance().SimulationTime;
        }


        private void InitInternal()
        {
            this.PopulateMembers();
            OnPropertyChanged("SimulationTime");
        }

    }
}
