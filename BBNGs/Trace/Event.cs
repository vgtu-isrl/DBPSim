using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;

namespace BBNGs.TraceLog
{
    [Serializable]
    public class Event
    {
        /// <summary>
        /// Additional event data not required for main processing
        /// </summary>
        public Dictionary<string, EventValue> EventVals = new Dictionary<string, EventValue>();
        /// <summary>
        /// timestamp of the event
        /// </summary>
        public DateTime timestamp;
        /// <summary>
        /// concept:name (activity name) of the event
        /// </summary>
        public string name;
        /// <summary>
        /// lifecycle:transition (transition type) of the event
        /// </summary>
        public string transition;
        /// <summary>
        /// Concatenated nametransition
        /// </summary>
        public string fullName;
        /// <summary>
        /// XES "event" xml fragment
        /// </summary>
        [NonSerialized]
        internal XElement e;
        /// <summary>
        /// Trace to which the event belongs to
        /// </summary>
        public Trace trace;

        /// <summary>
        /// Consutrcor of the event object
        /// </summary>
        /// <param name="e">XES "event" xml fragment</param>
        /// <param name="xes">main namespace of the log file</param>
        /// <param name="trace">Trace to which the event belongs to</param>
        internal Event(XElement e, XNamespace xes, Trace trace, List<string> toIgnore = null)
        {
            this.e = e;
            this.trace = trace;
            IEnumerable<XNode> nodes = e.Nodes();
            foreach (XNode eventNode in nodes)
            {
                ProcessEvent(trace, toIgnore, eventNode);
            }


            StringBuilder sb = new StringBuilder();
            for (var i = 0; i < name.Length; i++)
            {
                char l = name[i];
                if (l >= 'a' && l <= 'z' || l >= 'A' && l <= 'Z' || l >= '0' && l <= '9')
                {
                    sb.Append(l);
                }
            }
            name = sb.ToString();

            int month = this.timestamp.Month;

            if (month > 1 && month < 7)
            {
                ProcessEvent(trace, toIgnore, new XElement("string", new XAttribute("key", "YEARTIME"), new XAttribute("value", "SPRING")));
            }
            else if (month >= 9)
            {
                ProcessEvent(trace, toIgnore, new XElement("string", new XAttribute("key", "YEARTIME"), new XAttribute("value", "AUTUMN")));
            }
            else
            {
                ProcessEvent(trace, toIgnore, new XElement("string", new XAttribute("key", "YEARTIME"), new XAttribute("value", "SUMMER")));
            }
            //ProcessEvent(trace, toIgnore, new XElement("string", new XAttribute("key", "MONTH"), new XAttribute("value", this.timestamp.Month)));
            //ProcessEvent(trace, toIgnore, new XElement("string", new XAttribute("key", "STOJ_METAI"), new XAttribute("value", new String(trace.name.Split('_')[0].Take(4).ToArray()))));//this.EventVals["STUD_KODAS"].value)));

            var eIdx = 0;

            //for simplicity stores concatenated nametransition
            fullName = (name.ToLower() + transition.ToLower()).Replace(" ", "_");
        }

        private void ProcessEvent(Trace trace, List<string> toIgnore, XNode eventNode)
        {
            XElement el = eventNode as XElement;
            //extracts data type and value of the event attribute
            EventValue val = new EventValue();
            val.evel = el;
            val.type = el.Name.LocalName;
            val.value = el.Attribute("value").Value;
            string key = el.Attribute("key").Value;

            if(key=="duration")
            {
                int dur = Int32.Parse(val.value);
                int after = dur - dur % 5000;
                val.value = after.ToString();
            }
            //stores timestamp additionally in the object
            if (key == "time:timestamp")
                timestamp = DateTime.Parse(val.value);
            //stores event activity name additionally in the object
            else if ( key == "activityNameEN")
                name = val.value.Replace(' ', '_').ToLower();
            //stores transition type additionally in the object
            else if (key == "lifecycle:transition")
                transition = val.value;
            else if (key == "concept:name" && string.IsNullOrEmpty(name))
            {
                name = val.value.Replace(' ', '_').ToLower();
            }
            //stores attribute in the attribute list
            else if (val.value != trace.name)
                if (toIgnore == null)
                    EventVals.Add(key, val);
                else if (!toIgnore.Contains(key))
                    EventVals.Add(key, val);
            
        }

        public void AddEventIndex(int index)
        {
            //ProcessEvent(trace, new List<string>(),new XElement("string", new XAttribute("key", "EVIDX"), new XAttribute("value", index.ToString())));
        }


        public static string EventName(XElement e)
        {
            IEnumerable<XNode> nodes = e.Nodes();
            foreach (XNode eventNode in nodes)
            {
                XElement el = eventNode as XElement;
                //extracts data type and value of the event attribute
                EventValue val = new EventValue();
                val.evel = el;
                val.type = el.Name.LocalName;
                val.value = el.Attribute("value").Value;
                string key = el.Attribute("key").Value;
                //stores event activity name additionally in the object
                if (key == "concept:name")
                    return val.value.Replace(' ', '_').ToLower();

            }
            return String.Empty;
        }


        public bool TryToFindValue(string key, out string result)
        {
            EventValue res;
            if (EventVals.TryGetValue(key, out res))
            {
                result = res.value;
                return true;
            }
            result = null;
            return false;
        }

        public string Find(string key)
        {
            EventValue res;
            if (EventVals.TryGetValue(key, out res))
            {
                return res.value;
            }
            if (this.fullName.Contains("endcomplete"))
                return null;
            else
                return null;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return fullName;
        }

    }

    [Serializable]
    public class EventValue
    {
        public string type;
        public string value;
        [NonSerialized]
        public XElement evel;

        public EventValue()
        {

        }

        public EventValue(XElement nodeElement, string attribute, string value)
        {
            XElement el = new XElement("string", new XAttribute("key", attribute), new XElement("value", value));
            evel = el;
            nodeElement.Add(el);
            type = "string";
            this.value = value;
        }

    }

    class Comparer : IEqualityComparer<Event>
    {
        public bool Equals(Event x, Event y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(Event x)
        {
            return base.GetHashCode();
        }
    }
}
