using DBPSim.Events;
using DBPSim.Simulation;
using DBPSim.Simulation.ProcessModel;
using DBPSim.Simulation.Simulator;
using DBPSim.SimulationGUI.ModelDrawer;
using DBPSim.SimulationGUI.ProcessModel;
using DBPSim.SimulationGUI.ViewModels;
using DBPSim.SimulationGUI.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DBPSim.SimulationGUI.Tabs
{
    /// <summary>
    /// Interaction logic for TabItemSimulation.xaml
    /// </summary>
    public partial class TabItemSimulation : UserControl, IDisposable
    {

        public TabItemSimulation()
        {
            InitializeComponent();

            WorkingMemoryViewModel workingMemoryViewModel = new WorkingMemoryViewModel();
            this.GroupBoxWorkingMemory.DataContext = workingMemoryViewModel;

            this.GroupBoxProcessModelInstances.DataContext = ProcessModelInstancesViewModel.Instance;

            this.GroupBoxSimulationWatchPoints.DataContext = SimulationWatchPointViewModel.Instance;

            SimulationTimeViewModel simulatinTimeViewModel = new SimulationTimeViewModel();
            this.LabelTime.DataContext = simulatinTimeViewModel;

            SimulationEventProvider.ProvideEvents();
        }


        private void TextBoxSimulationLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.TextBoxSimulationLog.ScrollToEnd();
        }


        private void TextBoxWatchPointValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            WatchPointMember watchPointMember = (WatchPointMember)((TextBox)sender).DataContext;
            object value = Context.WatchingPoints[watchPointMember.Key];
            string newValue = ((TextBox)sender).Text;

            if (!string.IsNullOrEmpty(newValue) && this.ValidateValueByType(value, newValue))
            {
                if (value.GetType() == typeof(int))
                {
                    Context.WatchingPoints[watchPointMember.Key] = int.Parse(newValue);
                }
                else if (value.GetType() == typeof(double))
                {
                    Context.WatchingPoints[watchPointMember.Key] = double.Parse(newValue);
                }
                else
                {
                    Context.WatchingPoints[watchPointMember.Key] = newValue;
                }
            }
        }


        private void TextBoxWatchPointValue_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            WatchPointMember watchPointMember = (WatchPointMember)((TextBox)sender).DataContext;
            object value = Context.WatchingPoints[watchPointMember.Key];
            string newValue = ((TextBox)sender).Text;

            if (!string.IsNullOrEmpty(newValue))
            {
                if (!this.ValidateValueByType(value, e.Text))
                {
                    e.Handled = true;
                }
            }
        }


        private void ListBoxProcessModelInstances_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ProcessModelInstance processModel = (ProcessModelInstance)((ListBox)sender).SelectedItem;
            if (processModel != null)
            {
                SimulationModelDrawer.DrawSimulationModel(this.SimulationModel, processModel);
            }
            else
            {
                SimulationModelDrawer.ClearModel(this.SimulationModel);
            }
        }


        private void ListBoxProcessModelInstances_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ProcessModelInstance processModel = (ProcessModelInstance)((ListBox)sender).SelectedItem;
            SimulationModelDrawer.DrawSimulationModel(this.SimulationModel, processModel);
        }


        private bool ValidateValueByType(object value, string newValue)
        {
            if (value.GetType() == typeof(int))
            {
                int valueAsInteger;
                if (!int.TryParse(newValue, out valueAsInteger))
                {
                    return false;
                }
            }
            else if (value.GetType() == typeof(double))
            {
                double valueAsDouble;
                if (!double.TryParse(newValue, out valueAsDouble))
                {
                    return false;
                }
            }
            return true;
        }


        private bool ValidateSimulationCanBeStarted()
        {
            if (Context.Events == null || Context.Events.Count == 0)
            {
                MessageBox.Show("No events loaded.");
                return false;
            }
            else if (Context.Rules == null || Context.Rules.Count == 0)
            {
                MessageBox.Show("No rules loaded.");
                return false;
            }
            return true;
        }


        private void ComboBoxSimulationSpeed_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this._threadPauseMsTime = int.Parse((string)((ComboBoxItem)this.ComboBoxSimulationSpeed.SelectedValue).Tag);
        }


        private void ButtonEventInformation_Click(object sender, RoutedEventArgs e)
        {
            Event evnt = ((ProcessModelInstance)(((Button)sender).DataContext)).Event;

            EventInformation eventInformationWindow = new EventInformation(evnt);
            eventInformationWindow.ShowDialog();
        }


        private void ReDrawSimulationModel()
        {
            ProcessModelInstance processModel = this.ListBoxProcessModelInstances.SelectedItem as ProcessModelInstance;
            if (processModel != null)
            {
                SimulationModelDrawer.DrawSimulationModel(this.SimulationModel, processModel);
            }
            else
            {
                SimulationModelDrawer.ClearModel(this.SimulationModel);
            }
        }

        private void ClearSimulationModel()
        {
            SimulationModelDrawer.ClearModel(this.SimulationModel);
        }


        public void Dispose()
        {
            //((IDisposable)this.GroupBoxWorkingMemory.DataContext).Dispose();
            if (this._simulationThread != null)
            {
                this._simulationThread.Abort();
            }
        }


    }
}
