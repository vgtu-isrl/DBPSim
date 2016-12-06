using DBPSim.RuleEngine;
using DBPSim.RuleEngine.DataSource;
using DBPSim.RuleEngine.Execution;
using DBPSim.SimulationGUI.ViewModels;
using DBPSim.SimulationGUI.Windows;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace DBPSim.SimulationGUI.Tabs
{
    /// <summary>
    /// Interaction logic for TabItemRules.xaml
    /// </summary>
    public partial class TabItemRules : UserControl, IDisposable
    {

        public TabItemRules()
        {
            InitializeComponent();

            RulesViewModel rulesViewModel = new RulesViewModel();
            this.DataGridRules.DataContext = rulesViewModel;
        }


        public void Dispose()
        {
            ((IDisposable)this.DataGridRules.DataContext).Dispose();
        }


        private void ButtonShowConditionException_Click(object sender, RoutedEventArgs e)
        {
            ExecutionResult executionResult = (ExecutionResult)((Button)sender).DataContext;
            ExceptionWindow window = new ExceptionWindow(executionResult.Result.ConditionException.ToString());
            window.ShowDialog();
        }


        private void ButtonShowBodyException_Click(object sender, RoutedEventArgs e)
        {
            ExecutionResult executionResult = (ExecutionResult)((Button)sender).DataContext;
            ExceptionWindow window = new ExceptionWindow(executionResult.Result.BodyException.ToString());
            window.ShowDialog();
        }
        

    }
}
