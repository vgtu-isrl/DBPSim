using DBPSim.RuleEngine.Execution;
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
    public class RuleExecutionResultViewModel : INotifyPropertyChanged
    {

        private static RuleExecutionResultViewModel _viewModel;
        private ExecutionResultCollection _executionResultCollection;


        public RuleExecutionResultViewModel(ExecutionResultCollection executionResultCollection)
        {
            this._executionResultCollection = executionResultCollection;
            _viewModel = this;
            this.PopulateMembers();
        }


        public static void Init()
        {
            if (_viewModel != null)
            {
                _viewModel.InitInternal();
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;


        public ObservableCollection<ExecutionResult> ExecutionResults { get; set; }


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
            this.ExecutionResults = new ObservableCollection<ExecutionResult>();
            foreach (ExecutionResult executionResult in this._executionResultCollection)
            {
                this.ExecutionResults.Add(executionResult);
            }
        }


        private void InitInternal()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate() { this.PopulateMembers(); });
            //this.PopulateMembers();

            OnPropertyChanged("ExecutionResults");
        }

    }
}
