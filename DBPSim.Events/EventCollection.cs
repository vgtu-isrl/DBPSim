using DBPSim.RuleEngine;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Xml;

namespace DBPSim.Events
{
    public class EventCollection : List<Event>
    {

        public EventCollection()
        {
        }


        public static EventCollection FromXml(string xml)
        {
            EventCollection events = null;

            bool xmlLoaded = false;
            XmlDocument document = new XmlDocument();
            try
            {
                document.LoadXml(xml);
                xmlLoaded = true;
            }
            catch (XmlException)
            {
            }

            if (xmlLoaded)
            {
                XmlElement documentElement = document.DocumentElement;
                List<XmlElement> eventElements = XmlHelper.FindElements(documentElement, "Event");
                if (eventElements.Count > 0)
                {
                    events = new EventCollection();
                }
                foreach (XmlElement eventElement in eventElements)
                {
                    string name = XmlHelper.GetElementString(eventElement, "Name");
                    DateTime? eventDate = XmlHelper.GetElementAsDate(eventElement, "Date");
                    string configuration = XmlHelper.GetElementString(eventElement, "Configuration");

                    XmlElement expandoConfigurationElements = XmlHelper.FindElement(eventElement, "ExpandoConfiguration");

                    IDictionary<string, object> eventExpando = new ExpandoObject();

                    foreach (XmlElement expandoAttribute in expandoConfigurationElements.ChildNodes)
                    {
                        string expandoKey = expandoAttribute.Name;
                        string expandoValueTypeName = XmlHelper.GetElementString(expandoAttribute, "Type");
                        string expandoValueAsString = XmlHelper.GetElementString(expandoAttribute, "Value");

                        Type expandoType = Type.GetType(expandoValueTypeName);

                        object expandoValueAsObject = GetValueByType(expandoType, expandoValueAsString);

                        eventExpando.Add(expandoKey, expandoValueAsObject);                        
                    }

                    Event evnt = new Event(name, configuration, eventDate, (ExpandoObject)eventExpando);

                    events.Add(evnt);
                }
            }

            return events;
        }


        private static object GetValueByType(Type type, string value)
        {
            if (value == null)
            {
                return null;
            }

            if (type == typeof(string))
            {
                return value;
            }

            if (type == typeof(int))
            {
                return int.Parse(value);
            }

            if (type == typeof(double))
            {
                return double.Parse(value);
            }

            return null;
        }


        public void SortRandomly()
        {
            Random rng = new Random();
            int n = this.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                Event value = this[k];
                this[k] = this[n];
                this[n] = value;
            }
        }


        public Event GetFirstProcessingEvent()
        {
            return this.FirstOrDefault();
        }


        public Event GetProcessingEvent()
        {
            List<Event> evnts = this.FindAll(item => !item.Processed);
            if (evnts != null && evnts.Count > 0)
            {
                Event evnt = evnts.OrderBy(item => item.EventDate).First();
                evnt.SetEventStateToProcessed();
                return evnt;
            }
            return null;
        }


        public Event GetProcessingEvent(DateTime currentTime)
        {
            List<Event> evnts = this.FindAll(item => !item.Processed && item.EventDate >= currentTime);
            if (evnts != null && evnts.Count > 0)
            {
                Event evnt = evnts.OrderBy(item => item.EventDate).First();
                evnt.SetEventStateToProcessed();
                return evnt;
            }
            return null;
        }


        public void Restart()
        {
            foreach (Event evnt in this)
            {
                evnt.SetEventStateToNotProcessed();
            }
        }


        public string ToXml()
        {
            XmlDocument document = new XmlDocument();
            XmlElement documentElement = document.CreateElement("EventCollection");
            document.AppendChild(documentElement);

            foreach (Event evnt in this)
            {
                XmlElement eventElement = XmlHelper.CreateXmlElement(documentElement, "Event");

                XmlHelper.SaveValue(eventElement, "Name", evnt.Name);
                XmlHelper.SaveElement(eventElement, "Date", evnt.EventDate);
                XmlHelper.SaveValue(eventElement, "Configuration", evnt.Configuration);

                XmlElement eventExpandoelement = XmlHelper.CreateXmlElement(eventElement, "ExpandoConfiguration");

                IDictionary<string, object> expandoDictionary = (IDictionary<string, object>)evnt.EventExpando;
                foreach (KeyValuePair<string, object> keyValuePair in expandoDictionary)
                {
                    XmlElement expandoChild = XmlHelper.CreateXmlElement(eventExpandoelement, keyValuePair.Key);
                    XmlHelper.SaveValue(expandoChild, "Type", keyValuePair.Value.GetType().Namespace + "." + keyValuePair.Value.GetType().Name);
                    XmlHelper.SaveElement(expandoChild, "Value", keyValuePair.Value);
                }

            }
            return document.OuterXml;
        }

    }
}
