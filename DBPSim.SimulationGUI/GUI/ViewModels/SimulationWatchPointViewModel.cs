using DBPSim.Simulation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DBPSim.SimulationGUI.ViewModels
{

    public class SimulationWatchPointViewModel : INotifyPropertyChanged //, IDisposable
    {

        //private Thread WatchPointsChecker;
        private static SimulationWatchPointViewModel _viewModel;


        private SimulationWatchPointViewModel()
        {
            this.PopulateWatchPointMembers();
            _viewModel = this;
            //WatchPointsChecker = new Thread(new ThreadStart(RefreshValues));
            //WatchPointsChecker.Start();
        }


        public static SimulationWatchPointViewModel Instance
        {
            get
            {
                if (_viewModel == null)
                {
                    _viewModel = new SimulationWatchPointViewModel();
                }
                return _viewModel;
            }
        }


        public static void Init()
        {
            SimulationWatchPointViewModel.Instance.InitInternal();
        }


        public ObservableCollection<WatchPointMember> WatchPointMembers { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;


        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        //private void RefreshValues()
        //{
        //    while (true)
        //    {
        //        this.PopulateWatchPointMembers();
        //        OnPropertyChanged("WatchPointMembers");
        //        Thread.Sleep(50);
        //    }
        //}


        private void PopulateWatchPointMembers()
        {
            this.WatchPointMembers = new ObservableCollection<WatchPointMember>();

            foreach (KeyValuePair<string, object> keyValuePair in Context.WatchingPoints)
            {
                WatchPointMember watchPointMember = new WatchPointMember();
                watchPointMember.Key = keyValuePair.Key;
                watchPointMember.Value = keyValuePair.Value;
                this.WatchPointMembers.Add(watchPointMember);
            }
        }


        private void InitInternal()
        {
            this.PopulateWatchPointMembers();
            OnPropertyChanged("WatchPointMembers");
        }

        //public void Dispose()
        //{
        //    this.WatchPointsChecker.Abort();
        //}

    }
}
