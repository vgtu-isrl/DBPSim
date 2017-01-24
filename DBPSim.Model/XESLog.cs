using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DBPSim.Model
{
    public class XESLog
    {
        private XNamespace xesns = "http://www.xes-standard.org/";

        private List<XESTrace> traces = new List<XESTrace>();
        private XESTrace _currentTrace;
        private XESTrace currentTrace
        {
            get { if(_currentTrace == null)
                {
                    throw new ArgumentNullException("Empty Trace");
                }
                return _currentTrace;
            }
            set
            {
                traces.Add(value);
                _currentTrace = value;
            }
        }

        public XESLog()
        {
                        
        }

        public void AddTrace(string name)
        {
            currentTrace = new XESTrace();
            currentTrace.AddAttribute("string", "concept:name", name);
        }

        public void AddTraceData(string type, string key, string value)
        {
            currentTrace.AddAttribute(type, key, value);
        }

        public void AddEventData(string type, string key, string value)
        {
            currentTrace.currentEvent.AddAttribute(type, key, value);
        }

        public void AddEvent(string name, DateTime timestamp, string transition)
        {
            currentTrace.AddEvent(name,timestamp,transition);
        }

        public  XDocument Serialize(string filename=null)
        {
            XDocument doc = new XDocument(new XDeclaration("1.0", "utf-8", "no"));

            XElement root = new XElement(xesns+"log");
            root.Add(
                new XAttribute("xes.version","1.0"),
                new XAttribute("xes.features","nested-attributes"),
                new XAttribute("openxes.version","1.0RC7"),
                new XElement(xesns+"extension",
                    new XAttribute("name","Lifecycle"),
                    new XAttribute("prefix","lifecycle"),
                    new XAttribute("uri", "http://www.xes-standard.org/lifecycle.xesext")),
                new XElement(xesns + "extension",
                    new XAttribute("name", "Organizational"),
                    new XAttribute("prefix", "org"),
                    new XAttribute("uri", "http://www.xes-standard.org/org.xesext")),
                new XElement(xesns + "extension",
                    new XAttribute("name", "Time"),
                    new XAttribute("prefix", "time"),
                    new XAttribute("uri", "http://www.xes-standard.org/time.xesext")),
                new XElement(xesns + "extension",
                    new XAttribute("name", "Concept"),
                    new XAttribute("prefix", "concept"),
                    new XAttribute("uri", "http://www.xes-standard.org/concept.xesext")),
                new XElement(xesns + "extension",
                    new XAttribute("name", "Semantic"),
                    new XAttribute("prefix", "semantic"),
                    new XAttribute("uri", "http://www.xes-standard.org/semantic.xesext")),
                new XElement(xesns+"global",
                    new XAttribute("scope","trace"),
                    new XElement(xesns+"string",
                        new XAttribute("key","concept:name"),
                        new XAttribute("value", "__INVALID__"))),
                new XElement(xesns + "global",
                    new XAttribute("scope", "event"),
                    new XElement(xesns + "string",
                        new XAttribute("key", "concept:name"),
                        new XAttribute("value", "__INVALID__")),
                    new XElement(xesns + "string",
                        new XAttribute("key", "lifecycle:transition"),
                        new XAttribute("value", "complete"))),
                new XElement(xesns+"classifier",
                    new XAttribute("name", "MXML Legacy Classifier"),
                    new XAttribute("keys", "concept:name lifecycle:transition")),
                new XElement(xesns + "classifier",
                    new XAttribute("name", "Event Name"),
                    new XAttribute("keys", "concept:name")),
                new XElement(xesns + "classifier",
                    new XAttribute("name", "Resource"),
                    new XAttribute("keys", "org:resource"))
                );

            foreach (var trace in traces)
            {
                root.Add(trace.Serialize());
            }

            doc.Add(root);
            if (!string.IsNullOrEmpty(filename))
            {
                doc.Save(filename);
            }
            return doc;
        }


        class XESContainerElement
        {
            private XNamespace ns = "http://www.xes-standard.org/";

            public List<XESElement> Attributes = new List<XESElement>();
            public void AddAttribute(string type, string key, string value)
            {
                Attributes.Add(new XESElement(type, key, value));
            }
            

            public virtual XElement Serialize()
            {
                return Serialize("container");
            }

            protected XElement Serialize(string elementName)
            {
                XElement root = new XElement(ns + elementName);
                foreach (var attr in Attributes)
                {
                    var attrEl = new XElement(
                        ns + attr.Type,
                        new XAttribute("key", attr.Key),
                        new XAttribute("value", attr.Value)
                        );
                    root.Add(attrEl);
                }
                return root;
            }


        }

        class XESTrace:XESContainerElement
        {
            private List<XESEvent> events = new List<XESEvent>();

            private XESEvent _currentEvent;
            public XESEvent currentEvent
            {
                get
                {
                    if (_currentEvent == null)
                    {
                        throw new ArgumentNullException("Empty Event");
                    }
                    return _currentEvent;
                }
                set
                {
                    events.Add(value);
                    _currentEvent = value;
                }
            }

            public void AddEvent(string name, DateTime timestamp, string transition)
            {
                currentEvent = new XESEvent();
                currentEvent.AddAttribute("string", "concept:name", name);
                currentEvent.AddAttribute("date", "time:timestamp", timestamp.ToString("O"));
                currentEvent.AddAttribute("string", "lifecycle:transition", transition);

            }

            public override XElement Serialize()
            {
                var root = Serialize("trace");
                foreach (var ev in events)
                {
                    root.Add(ev.Serialize());
                }
                return root;
            }

        }
        
        class XESEvent:XESContainerElement
        {
            public override XElement Serialize()
            {
                return base.Serialize("event");
            }
        }

        class XESElement
        {
            public string Type { get; set; }
            public string Key { get; set; }
            public string Value { get; set; }

            public XESElement(string type, string key, object value)
            {
                Type = type;
                Key = key;
                Value = value.ToString();
            }
        }

    }

}
