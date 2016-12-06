using System;
using System.ComponentModel;

namespace DBPSim.SimulationGUI.ViewModels
{
    public class SimulationLogViewModel
    {

        private static string _logText = string.Empty;
        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;


        public static void WriteLine(string sText)
        {
            LogText += sText + "\n";
        }


        public static void WriteLineWithDate(string sText)
        {
            LogText += string.Format("{0} {1} \n", DateTime.Now, sText);
        }


        public static void WriteLineWithDate(string sText, params object[] parameters)
        {
            string logMsg = string.Format("{0} {1} \n", DateTime.Now, sText);
            LogText += string.Format(logMsg, parameters);
        }


        public static void WriteObject(object obj, params object[] parameters)
        {
            if (obj == null)
            {
                WriteLineWithDate("null");
            }
            string objAsString = obj.ToString();
            string sText = string.Format(objAsString, parameters);
            WriteLineWithDate(sText);
        }


        public static void Refresh()
        {
            LogText = string.Format("{0} Simulation restarted. \n", DateTime.Now);
        }


        public static string LogText
        {
            get { return _logText; }
            set
            {
                if (value != _logText)
                {
                    _logText = value;

                    NotifyStaticPropertyChanged("LogText");
                }
            }
        }


        private static void NotifyStaticPropertyChanged(string propertyName)
        {
            if (StaticPropertyChanged != null)
            {
                StaticPropertyChanged(null, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
