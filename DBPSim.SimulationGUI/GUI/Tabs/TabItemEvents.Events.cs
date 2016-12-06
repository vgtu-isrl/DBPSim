using DBPSim.Events;
using DBPSim.SimulationGUI.ViewModels;
using DBPSim.SimulationGUI.Windows;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using DBPSim.Simulation;

namespace DBPSim.SimulationGUI.Tabs
{
    public partial class TabItemEvents
    {

        private void LoadEventsFromFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".evs";
            dlg.Filter = "Event files (*.evs)|*.evs|All files (*.*)|*.*";

            if (dlg.ShowDialog() == true)
            {
                string fullFilePath = dlg.FileName;
                EventCollection events = EventCollection.FromXml(File.ReadAllText(fullFilePath));
                if (events != null)
                {
                    Context.Events = events;
                    EventsViewModel.Init();
                }
                else
                {
                    MessageBox.Show("Wrong file format.");
                }
            }
        }


        private void SaveEventsToFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".evs";
            dlg.Filter = "Event files (*.evs)|*.evs|All files (*.*)|*.*";
            if (dlg.ShowDialog() == true)
            {
                string fullFilePath = dlg.FileName;

                string eventsXml = Context.Events.ToXml();

                File.WriteAllText(fullFilePath, eventsXml);
            }
        }


        private void CheckBoxEnableDate_Checked(object sender, RoutedEventArgs e)
        {
            this.DatePickerDateFrom.IsEnabled = true;
            this.DatePickerDateTo.IsEnabled = true;
            this.ComboBoxTimeFrom.IsEnabled = true;
            this.ComboBoxTimeTo.IsEnabled = true;
        }


        private void CheckBoxEnableDate_Unchecked(object sender, RoutedEventArgs e)
        {
            this.DatePickerDateFrom.IsEnabled = false;
            this.DatePickerDateTo.IsEnabled = false;
            this.ComboBoxTimeFrom.IsEnabled = false;
            this.ComboBoxTimeTo.IsEnabled = false;
        }
        //
        private void CheckBoxEnableStartDate_Checked(object sender, RoutedEventArgs e)
        {
            this.DatePickerStartDate.IsEnabled = true;
            this.ComboBoxStartDateHours.IsEnabled = true;
        }


        private void CheckBoxEnableStartDate_Unchecked(object sender, RoutedEventArgs e)
        {
            this.DatePickerStartDate.IsEnabled = false;
            this.ComboBoxStartDateHours.IsEnabled = false;
        }

        //
        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            string errorMsg = null;
            if (!this.ValidateEventForm(out errorMsg))
            {
                MessageBox.Show("Unable to add event because there are errors in configuration. Please click Validate to check errors.");
                return;
            }

            int eventsCount = int.Parse(this.TextBoxGenerateEventsCount.Text);

            for (int i = 0; i < eventsCount; i++)
            {
                Event evnt = EventGenerator.GenerateEvent(this.TextBoxEventName.Text, this.TextBoxEventDataConfiguration.Text, this.GetConfigurationDate());
                if (evnt != null)
                {
                    Context.Events.Add(evnt);
                }
            }

            EventsViewModel.Init();
        }


        private void ButtonValidate_Click(object sender, RoutedEventArgs e)
        {
            string errorMsg = null;
            if (!this.ValidateEventForm(out errorMsg))
            {
                this.TextBlockErrors.Text = errorMsg;
            }
            else
            {
                this.TextBlockErrors.Text = "Event configuration is good.";
            }
        }


        private void ButtonShowFull_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string translatedRule = EventSyntaxValidator.GetFullRule(this.TextBoxEventDataConfiguration.Text);

            FullRule dialog = new FullRule(translatedRule);
            dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            dialog.ShowDialog();
        }


        private void ButtonSortRandomly_Click(object sender, RoutedEventArgs e)
        {
            Context.Events.SortRandomly();
            EventsViewModel.Init();
        }


        private void ButtonSort_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Context.Events.Sort(delegate(Event p1, Event p2)
                                {
                                    try
                                    {
                                        if (!p1.EventDate.HasValue || !p2.EventDate.HasValue)
                                        {
                                            return -1;
                                        }
                                        if (p1.EventDate.Value == p2.EventDate.Value)
                                        {
                                            return 0;
                                        }
                                        if (p1.EventDate.Value > p2.EventDate.Value)
                                        {
                                            return 1;
                                        }
                                        return -1;
                                    }
                                    catch
                                    {
                                        return -1;
                                    }
                                }
            );
            EventsViewModel.Init();
        }


        private void ButtonClearAllEvents_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Context.Events.Clear();
            EventsViewModel.Init();
        }


        private void ButtonDeleteEvent_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            EventViewModelMember eventMember = (EventViewModelMember)((Button)sender).DataContext;
            Context.Events.Remove(eventMember.Event);
            EventsViewModel.Init();
        }

    }
}
