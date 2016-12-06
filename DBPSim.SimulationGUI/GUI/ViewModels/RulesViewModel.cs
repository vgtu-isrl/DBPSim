using DBPSim.RuleEngine;
using DBPSim.Simulation;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;

namespace DBPSim.SimulationGUI.ViewModels
{
    public class RulesViewModel : INotifyPropertyChanged, IDisposable
    {

        //private Thread RulesChecker;
        public event PropertyChangedEventHandler PropertyChanged;        

        private static RulesViewModel _instance = null;

        public RulesViewModel()
        {
            this.PopulateRulesCollection();

            _instance = this;
            //this.RulesChecker = new Thread(new ThreadStart(RefreshValues));
            //this.RulesChecker.Start();
        }


        public ObservableCollection<RuleBase> Rules
        {
            get;
            set;
        }


        public static void Init()
        {
            _instance.PopulateRulesCollection();
            _instance.OnPropertyChanged("Rules");
        }


        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        private void PopulateRulesCollection()
        {
            this.Rules = new ObservableCollection<RuleBase>(Context.Rules);
        }


        private void RefreshValues()
        {
            //while (true)
            //{
                this.PopulateRulesCollection();
                OnPropertyChanged("Rules");
                //Thread.Sleep(1000);
            //}
        }


        public void Dispose()
        {
            _instance = null;
            //this.RulesChecker.Abort();
        }

    }
}

