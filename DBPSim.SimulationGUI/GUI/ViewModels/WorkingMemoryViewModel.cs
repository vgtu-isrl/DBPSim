using DBPSim.Simulation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace DBPSim.SimulationGUI.ViewModels
{

    public class WorkingMemoryViewModel : INotifyPropertyChanged //, IDisposable
    {

        //private Thread WorkingMemoryChecker;

        private static WorkingMemoryViewModel _viewModelInstance;


        public WorkingMemoryViewModel()
        {
            this.PopulateWorkingMemoryMembers();

            //WorkingMemoryChecker = new Thread(new ThreadStart(RefreshValues));
            //WorkingMemoryChecker.Start();
            _viewModelInstance = this;
        }


        public static void Init()
        {
            if (_viewModelInstance != null)
            {
                _viewModelInstance.InitInternal();
            }
        }


        public ObservableCollection<WorkingMemoryMember> WorkingMemoryMembers { get; set; }


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
        //        System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate() { this.PopulateWorkingMemoryMembers(); });

        //        //this.PopulateWorkingMemoryMembers();

        //        OnPropertyChanged("WorkingMemoryMembers");
        //        Thread.Sleep(50);
        //    }
        //}


        private void PopulateWorkingMemoryMembers()
        {
            ObservableCollection<WorkingMemoryMember> WorkingMemoryMembersBefore = WorkingMemoryMembers;

            this.WorkingMemoryMembers = new ObservableCollection<WorkingMemoryMember>();

            foreach (string sKey in Context.Memory.Keys)
            {
                WorkingMemoryMember workingMemberBefore = WorkingMemoryMembersBefore == null ? null : WorkingMemoryMembersBefore.FirstOrDefault(item => item.Key == sKey);

                WorkingMemoryMember workingMemberNow = new WorkingMemoryMember();
                workingMemberNow.Key = sKey;

                IDictionary<string, object> propertyValues = Context.Memory[sKey] as IDictionary<string, object>;
                if(propertyValues == null)
                {
                    continue;
                }

                foreach (var value in propertyValues)
                {
                    WorkingMemoryMemberProperty propertyNow = new WorkingMemoryMemberProperty();
                    propertyNow.PropertyName = value.Key;
                    propertyNow.PropertyValue = value.Value;

                    if (workingMemberBefore != null)
                    {
                        WorkingMemoryMemberProperty propertyBefore = workingMemberBefore.Properties.FirstOrDefault(item => item.PropertyName == propertyNow.PropertyName);
                        if (propertyBefore != null && propertyNow.PropertyValue != propertyBefore.PropertyValue)
                        {
                            propertyNow.ForegroundColor = System.Windows.Media.Brushes.Red;
                            propertyNow.CurrentFontWeight = System.Windows.FontWeights.Bold;
                        }
                    }

                    workingMemberNow.Properties.Add(propertyNow);
                }

                this.WorkingMemoryMembers.Add(workingMemberNow);
            }
        }


        private void InitInternal()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate() { this.PopulateWorkingMemoryMembers(); });
            //this.PopulateWorkingMemoryMembers();

            OnPropertyChanged("WorkingMemoryMembers");
        }


        //public void Dispose()
        //{
        //    this.WorkingMemoryChecker.Abort();
        //}

    }

}
