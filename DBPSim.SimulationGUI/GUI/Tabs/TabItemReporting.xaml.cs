using DBPSim.Events;
using DBPSim.Simulation;
using DBPSim.Simulation.ProcessModel;
using DBPSim.SimulationGUI.ProcessModel;
using DBPSim.SimulationGUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization.Charting;
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
    /// Interaction logic for TabItemReporting.xaml
    /// </summary>
    public partial class TabItemReporting : UserControl
    {

        private class ReportGraphValueList : List<ReportGraphValue>
        {
            public void Add(long key, object value)
            {
                this.Add(new ReportGraphValue(key, value));
            }

        }

        private class ReportGraphValue
        {
            public long Key { get; private set; }
            public object Value { get; private set; }

            public ReportGraphValue(long key, object value)
            {
                this.Key = key;
                this.Value = value;
            }

            public override string ToString()
            {
                return String.Format("{0} {1}", this.Key, this.Value);
            }
        }

        ReportGraphValueList reportGraph = new ReportGraphValueList();

        public TabItemReporting()
        {
            InitializeComponent();

            this.ListBoxWatchPoints.DataContext = SimulationWatchPointViewModel.Instance;
            this.DataValueSet.DataContext = this;

            this.ListBoxProcessInstances.DataContext = ProcessModelInstancesViewModel.Instance;
        }



        private void CheckBoxProcessInstance_Checked(object sender, RoutedEventArgs e)
        {
            this.ListBoxProcessInstances.IsEnabled = true;
        }


        private void CheckBoxProcessInstance_Unchecked(object sender, RoutedEventArgs e)
        {
            this.ListBoxProcessInstances.IsEnabled = false;
        }


        private void ListBoxWatchPoints_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            WatchPointMember watchPoint = (WatchPointMember)((ListBox)sender).SelectedItem;
            this.DrawGraph(watchPoint);
        }


        private void ListBoxWatchPoints_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            WatchPointMember watchPoint = (WatchPointMember)((ListBox)sender).SelectedItem;
            this.DrawGraph(watchPoint);
        }


        private void CheckBoxUseSimulationTime_Checked(object sender, RoutedEventArgs e)
        {
            this.RedrawGraph();
        }


        private void CheckBoxUseSimulationTime_Unchecked(object sender, RoutedEventArgs e)
        {
            this.RedrawGraph();
        }


        private void RedrawGraph()
        {
            WatchPointMember watchPoint = (WatchPointMember)this.ListBoxWatchPoints.SelectedItem;
            if (this.CheckBoxProcessInstance.IsChecked ?? false)
            {
                ProcessModelInstance processInstance = (ProcessModelInstance)this.ListBoxProcessInstances.SelectedItem;
                this.DrawProcessInstanceGraph(processInstance, watchPoint);
            }
            else
            {
                this.DrawGraph(watchPoint);
            }
        }


        private void FillReportGraphRecursively(ProcessAction action, ReportGraphValueList reportGraph, string key)
        {
            foreach (KeyValuePair<string, object> watchPointMember in action.WatchPointInstance.ExpandoObject)
            {
                if (watchPointMember.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (this.CheckBoxUseSimulationTime.IsChecked ?? true)
                    {
                        if (action.SimulationTime != null )// && !reportGraph.ContainsKey(action.SimulationTime.Value))
                        {
                            reportGraph.Add(new ReportGraphValue(action.SimulationTime.Value, watchPointMember.Value));
                        }
                    }
                    else
                    {
                        var time = (long)((action.Created - new DateTime(1970, 1, 1)).TotalMinutes);
                        //if (!reportGraph.ContainsKey(time))
                        //{
                        reportGraph.Add(new ReportGraphValue(time, watchPointMember.Value));
                        //}
                    }
                }
            }
            foreach (ProcessAction processAction in action.ProcessActions)
            {
                this.FillReportGraphRecursively(processAction, reportGraph, key);
            }
        }


        private void DrawGraph(WatchPointMember watchPoint)
        {
            if (watchPoint != null)
            {
                reportGraph = new ReportGraphValueList();

                foreach (ProcessModelInstance processModelInstance in Context.ProcessModelInstances)
                {
                    ProcessAction firstProcessAction = processModelInstance.FirstProcessAction;
                    if (firstProcessAction != null)
                    {
                        this.FillReportGraphRecursively(firstProcessAction, reportGraph, watchPoint.Key);
                    }
                }

                ((LineSeries)this.ChartReport.Series[0]).ItemsSource = reportGraph;
                this.DataValueSet.ItemsSource = reportGraph;
            }
        }


        private void ListBoxProcessInstances_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ProcessModelInstance processModel = (ProcessModelInstance)((ListBox)sender).SelectedItem;
            WatchPointMember watchPoint = (WatchPointMember)this.ListBoxWatchPoints.SelectedItem;

            this.DrawProcessInstanceGraph(processModel, watchPoint);
        }


        private void ListBoxProcessInstances_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ProcessModelInstance processModel = (ProcessModelInstance)((ListBox)sender).SelectedItem;
            WatchPointMember watchPoint = (WatchPointMember)this.ListBoxWatchPoints.SelectedItem;

            this.DrawProcessInstanceGraph(processModel, watchPoint);
        }


        private void DrawProcessInstanceGraph(ProcessModelInstance processModel, WatchPointMember watchPoint)
        {
            if (processModel != null && watchPoint != null)
            {
                reportGraph = new ReportGraphValueList();

                ProcessAction firstProcessAction = processModel.FirstProcessAction;
                if (firstProcessAction != null)
                {
                    this.FillReportGraphRecursively(firstProcessAction, reportGraph, watchPoint.Key);
                }
                ((LineSeries)this.ChartReport.Series[0]).ItemsSource = reportGraph;
                this.DataValueSet.ItemsSource = reportGraph;
            }
        }

    }
}
