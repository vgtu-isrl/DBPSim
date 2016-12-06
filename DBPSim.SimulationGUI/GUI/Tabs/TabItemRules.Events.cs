using DBPSim.RuleEngine;
using DBPSim.RuleEngine.DataSource;
using DBPSim.Simulation;
using DBPSim.SimulationGUI.ViewModels;
using DBPSim.SimulationGUI.Windows;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace DBPSim.SimulationGUI.Tabs
{
    public partial class TabItemRules
    {

        private string _fullFilePath {get;set;}

        private void ButtonLoadRulesFromFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".acts";
            dlg.Filter = "Activity files (*.acts)|*.acts|All files (*.*)|*.*";

            if (dlg.ShowDialog() == true)
            {
                _fullFilePath = dlg.FileName;
                XmlDataSourceProvider provider = new XmlDataSourceProvider(File.ReadAllText(_fullFilePath));
                Context.Rules = provider.Load();
                RulesViewModel.Init();
            }
        }


        public void RefreshRules()
        {
            RulesViewModel.Init();
        }


        private void ButtonSaveRulesInFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".acts";
            dlg.Filter = "Activity files (*.acts)|*.acts|All files (*.*)|*.*";
            if (dlg.ShowDialog() == true)
            {
                _fullFilePath = dlg.FileName;

                XmlDataSourceProvider provider = new XmlDataSourceProvider();
                provider.Save(Context.Rules);

                File.WriteAllText(_fullFilePath, provider.ExportedXml);
            }
        }

        private void ButtonSaveRules_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_fullFilePath))
            {
                XmlDataSourceProvider provider = new XmlDataSourceProvider();
                provider.Save(Context.Rules);

                File.WriteAllText(_fullFilePath, provider.ExportedXml);
            }
            else
            {
                MessageBox.Show("No file used yet.");
            }
        }


        private void ButtonAddRule_Click(object sender, RoutedEventArgs e)
        {
            RuleEditor dialog = new RuleEditor();
            dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            dialog.ShowDialog();
            RulesViewModel.Init();
            //((RulesViewModel)this.DataGridRules.DataContext).Init();
        }


        private void ButtonEditRule_Click(object sender, RoutedEventArgs e)
        {
            RuleEditor dialog = new RuleEditor((RuleConditional)((Button)sender).DataContext);
            dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            dialog.ShowDialog();

            RulesViewModel.Init();
            //((RulesViewModel)this.DataGridRules.DataContext).Init();
        }


        private void ButtonDeleteRule_Click(object sender, RoutedEventArgs e)
        {
            Context.Rules.Remove((RuleConditional)((Button)sender).DataContext);

            RulesViewModel.Init();
            //((RulesViewModel)this.DataGridRules.DataContext).Init();
        }


        private void RuleEnabled_Checked(object sender, RoutedEventArgs e)
        {
            RuleConditional rule = (RuleConditional)((CheckBox)sender).DataContext;
            rule.Enabled = ((CheckBox)sender).IsEnabled;
        }


        private void DataGridRules_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RuleConditional rule = (RuleConditional)((DataGrid)sender).SelectedItem;
            if (rule != null)
            {
                this.GridRuleExecutionLog.DataContext = new RuleExecutionResultViewModel(rule.ExecutionResultCollection);
            }
        }

    }
}
