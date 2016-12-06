using DBPSim.Events;
using DBPSim.Simulation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace DBPSim.SimulationGUI.ViewModels
{
    public class EventsViewModel : INotifyPropertyChanged
    {

        private static EventsViewModel _viewModel;


        public EventsViewModel()
        {
            this.PopulateMembers();
            _viewModel = this;
        }


        public static void Init()
        {
            if (_viewModel != null)
            {
                _viewModel.InitInternal();
            }
        }


        public ObservableCollection<EventViewModelMember> EventMembers { get; set; }


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
            this.EventMembers = new ObservableCollection<EventViewModelMember>();

            foreach (Event evnt in Context.Events)
            {
                EventViewModelMember viewModelMember = new EventViewModelMember();
                viewModelMember.Event = evnt;
                viewModelMember.Name = evnt.Name;
                viewModelMember.EventDate = evnt.EventDate;
                viewModelMember.Processed = evnt.Processed;
                viewModelMember.Fired = evnt.Fired;

                IDictionary<string, object> expandoDictionary = (IDictionary<string, object>)evnt.EventExpando;
                foreach (KeyValuePair<string, object> expandoDictionaryValuePair in expandoDictionary)
                {
                    EventViewModelPropertyMember viewModelPropertyMember = new EventViewModelPropertyMember();
                    viewModelPropertyMember.PropertyKey = expandoDictionaryValuePair.Key;
                    viewModelPropertyMember.PropertyValue = expandoDictionaryValuePair.Value.ToString();

                    viewModelMember.Properties.Add(viewModelPropertyMember);
                }

                this.EventMembers.Add(viewModelMember);
            }
        }


        private void InitInternal()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate() { this.PopulateMembers(); });
            OnPropertyChanged("EventMembers");
        }

    }
}
