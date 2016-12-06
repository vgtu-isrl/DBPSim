using DBPSim.Events;
using DBPSim.SimulationGUI.ViewModels;
using System;
using System.CodeDom.Compiler;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;

namespace DBPSim.SimulationGUI.Tabs
{
    /// <summary>
    /// Interaction logic for TabItemEvents.xaml
    /// </summary>
    public partial class TabItemEvents : UserControl
    {

        public TabItemEvents()
        {
            InitializeComponent();

            this.DatePickerDateFrom.SelectedDate = DateTime.Now;
            this.DatePickerDateTo.SelectedDate = DateTime.Now.AddDays(1);

            this.GroupBoxEvents.DataContext = new EventsViewModel();
        }


        private void TextBoxGenerateEventsCount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            int result;
            if (!int.TryParse(e.Text, out result))
            {
                e.Handled = true;
            }
        }


        private bool ValidateEventForm(out string errorMsg)
        {
            errorMsg = string.Empty;

            if (string.IsNullOrEmpty(this.TextBoxEventName.Text))
            {
                errorMsg += "Event name cannot be empty.\n\n";
            }

            if (string.IsNullOrEmpty(this.TextBoxEventDataConfiguration.Text))
            {
                errorMsg += "Event configuration cannot be empty.\n\n";
                return false;
            }

            CompilerErrorCollection compilerErrors;
            if (!EventSyntaxValidator.Validate(this.TextBoxEventDataConfiguration.Text, out compilerErrors))
            {
                StringBuilder sb = new StringBuilder();
                foreach (CompilerError compilerError in compilerErrors)
                {
                    sb.AppendFormat("Error number: {0}", compilerError.ErrorNumber);
                    sb.AppendLine();
                    sb.AppendFormat("Line: {0}", compilerError.Line);
                    sb.AppendLine();
                    sb.AppendFormat("Text: {0}", compilerError.ErrorText);
                    sb.AppendLine();
                    sb.Append("---------------------------------------------------------");
                    sb.AppendLine();
                }
                errorMsg = sb.ToString();
                return false;
            }
            Event evnt = EventGenerator.GenerateEvent("Test", this.TextBoxEventDataConfiguration.Text, null);
            if (evnt == null)
            {
                errorMsg = "Unexpected error occured.";
                return false;
            }

            if (this.CheckBoxEnableDate.IsChecked != null && this.CheckBoxEnableDate.IsChecked.Value)
            {
                //this.CheckBoxEnableStartDate.IsEnabled = false;
                DateTime? dateFrom = this.DatePickerDateFrom.SelectedDate;
                DateTime? dateTo = this.DatePickerDateTo.SelectedDate;
                if (dateFrom == null)
                {
                    errorMsg = "Date from must be not null.\n";
                    return false;
                }
                else if (dateTo == null)
                {
                    errorMsg = "Date to must be not null.\n";
                    return false;
                }
                else if (dateFrom >= dateTo)
                {
                    errorMsg = "Date to must be greater than date from\n";
                    return false;
                }

                int timeFrom = int.Parse((string)((ComboBoxItem)this.ComboBoxTimeFrom.SelectedValue).Tag);
                int timeTo = int.Parse((string)((ComboBoxItem)this.ComboBoxTimeTo.SelectedValue).Tag);
                if (timeFrom >= timeTo)
                {
                    errorMsg = "Time to must be greater than time from";
                    return false;
                }
            }

            int eventsCount;
            if (int.TryParse(this.TextBoxGenerateEventsCount.Text, out eventsCount))
            {
                if (eventsCount > 100)
                {
                    errorMsg = "Cannot generate more than 100 events at the time.";
                    return false;
                }
                if (eventsCount < 1)
                {
                    errorMsg = "Must generate one or at least one event.";
                    return false;
                }
            }

            return true;
        }


        private DateTime? GetConfigurationDate()
        {
            if (this.CheckBoxEnableDate.IsChecked == null || !this.CheckBoxEnableDate.IsChecked.Value)
            {
                return null;
            }

            DateTime dateFrom = this.DatePickerDateFrom.SelectedDate.Value;
            DateTime dateTo = this.DatePickerDateTo.SelectedDate.Value;

            int hourFrom = int.Parse((string)((ComboBoxItem)this.ComboBoxTimeFrom.SelectedValue).Tag);
            int hourTo = int.Parse((string)((ComboBoxItem)this.ComboBoxTimeTo.SelectedValue).Tag);

            DateTime dateFromWithTime = new DateTime(dateFrom.Year, dateFrom.Month, dateFrom.Day, hourFrom, 0, 0);
            DateTime dateToWithTime = new DateTime(dateTo.Year, dateTo.Month, dateTo.Day, hourTo, 0, 0);

            return EventGenerator.GetRandomDate(dateFromWithTime, dateToWithTime, hourFrom, hourTo);
        }        

    }
}
