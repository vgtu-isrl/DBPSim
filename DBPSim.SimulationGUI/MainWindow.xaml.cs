using DBPSim.RuleEngine;
using DBPSim.Simulation;
using DBPSim.Simulation.Simulator.SimulationFunctions;
using DBPSim.SimulationGUI.Windows.Menu;
using System;
using System.Windows;

namespace DBPSim.SimulationGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private static bool _ruleEngineInited = false;


        public MainWindow()
        {
            InitializeComponent();
            this.InitRuleEngine();
        }


        private void TabItemRules_Loaded(object sender, RoutedEventArgs e)
        {
        }


        private void Simulation_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ((IDisposable)this.TabItemRules).Dispose();
            ((IDisposable)this.TabItemSimulation).Dispose();
        }


        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private void MenuItemAbout_Click(object sender, RoutedEventArgs e)
        {
            About dialog = new About();
            dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            dialog.ShowDialog();
        }


        private void ClearContext_Click(object sender, RoutedEventArgs e)
        {
            Context.Init();
        }


        private void InitRuleEngine()
        {
            if (!_ruleEngineInited)
            {
                SimulationFunctionsProvider.ProvideFunctions();
                _ruleEngineInited = true;
            }
        }

    }
}
