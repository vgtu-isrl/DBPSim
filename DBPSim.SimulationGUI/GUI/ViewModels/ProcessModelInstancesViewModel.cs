using DBPSim.Simulation;
using DBPSim.Simulation.ProcessModel;
using DBPSim.SimulationGUI.ProcessModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace DBPSim.SimulationGUI.ViewModels
{

    public class ProcessModelInstancesViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        private static ProcessModelInstancesViewModel _viewModelInstance;


        private ProcessModelInstancesViewModel()
        {
            this.PopulateRulesCollection();
        }


        public ObservableCollection<ProcessModelInstance> ProcessModelInstances
        {
            get;
            set;
        }


        public ProcessModelInstance SelectedProcessModel
        {
            get;
            set;
        }


        public static ProcessModelInstancesViewModel Instance
        {
            get
            {
                if (_viewModelInstance == null)
                {
                    _viewModelInstance = new ProcessModelInstancesViewModel();
                }
                return _viewModelInstance;
            }
        }


        public static void Init()
        {
            if (_viewModelInstance != null)
            {
                _viewModelInstance.InitInternal();
            }
        }


        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        private void InitInternal()
        {
            this.PopulateRulesCollection();
            ProcessModelInstance processModel = Context.ProcessModelInstances.FindLast(item => item != null);
            this.SelectedProcessModel = processModel;
            OnPropertyChanged("ProcessModelInstances");
            OnPropertyChanged("SelectedProcessModel");
        }


        private void PopulateRulesCollection()
        {
            // Uncomment this line if you want to see all process model instances (including empty)
            //this.ProcessModelInstances = new ObservableCollection<ProcessModelInstance>(Context.ProcessModelInstances);
            this.ProcessModelInstances = new ObservableCollection<ProcessModelInstance>(Context.ProcessModelInstances.Where(item => item.FirstProcessAction != null));
        }

    }

}
