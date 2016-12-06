using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;


namespace AuxiliaryTasks
{
    class Program
    {
        static void Main(string[] args)
        {
           

            ClearLog();
        }

        private static void ClearLog()
        {
            XNamespace ns = "http://www.xes-standard.org/";
            string dir = @"F:\VGTUProj\DBPSim\BBNGs\bin\Debug\BPIC15_5.xes";

            var doc = XDocument.Load(dir);
            var traces = doc.Element(ns+"log").Elements(ns + "trace").ToList();

            var i = 0;
            List<string> uniques = new List<string>();
            foreach (var trace in traces)
            {
                
                Console.Write("\r");
                Console.Write("      ");
                Console.Write(i);
                i++;

                var evs = trace.Elements(ns + "event");
                var removed = false;
                foreach (var ev in evs)
                {
                    var bad = false;

                    var evlifecycle = ev.Elements().Where(x => x.Attribute("key").Value == "lifecycle:transition").FirstOrDefault();
                    if (evlifecycle == null || evlifecycle.Attribute("value").Value != "complete")
                    {
                        ev.Remove();
                    }


                    if (bad)
                    {
                        trace.Remove();
                        removed = true;
                        break;
                    }
                    else
                    {
                        var x = 0;
                    }
                }
                if (removed)
                {
                    continue;
                }
            }

            doc.Save(@"F:\VGTUProj\DBPSim\BBNGs\bin\Debug\BPIC_15_5_1.xes");
            Console.WriteLine("done");
            Console.ReadLine();
        }

        private static void UpdateLog()
        {
            XNamespace ns = "http://www.xes-standard.org/";
            string dir = @"D:\Projects\DBP-Simulation-Tool\SimulationTool-Source\BBNGs\bin\Debug\1_edit_old1.xes";

            var doc = XDocument.Load(dir);
            var traces= doc.Root.Elements(ns + "trace");
            List<string> prevs = new List<string>();
            foreach (var trace in traces)
            {
                var evs = trace.Elements(ns + "event");
                XElement prev = null;
                foreach (var ev in evs)
                {
                    var payment = ev.Elements(ns + "string").FirstOrDefault(x => x.Attribute("value").Value == "close claim") != null;
                    if (payment)
                    {
                        prevs.Add(prev.Elements(ns + "string").FirstOrDefault(x => x.Attribute("key").Value == "employee").Attribute("value").Value);
                    }
                    prev = ev;
                }
            }

            var grp = prevs.GroupBy(x => x).Select(x=> new { key = x.Key, val = x.Count() }).OrderByDescending(x=>x.val).ToList();

            Console.ReadLine();


        }

        private static void GenerateLog()
        {
            string dir = Environment.CurrentDirectory+@"\history_YYYY53DD2301SS.txt";

            List<KeyValuePair<string, string>> evs = new List<KeyValuePair<string, string>>();
            using (StreamReader sr = new StreamReader(dir))
            {
                while (!sr.EndOfStream)
                {
                    string[] ln = sr.ReadLine().Split('\t');
                    string instid = ln[0];
                    string evt = ln[1];

                    evs.Add(new KeyValuePair<string, string>(instid, evt));
                }
            }

            var x = from i in evs group i by i.Key into j select new { instance = j.Key, events = j.ToList() };
            int idx = 0;
            XElement root = new XElement("traces");
            DateTime dt = DateTime.Now;
            foreach (var trace in x)
            {
                XElement trc = new XElement("trace",

                        new XElement("string", new XAttribute("key", "concept:name"), new XAttribute("value", idx++)),
                        new XElement("string", new XAttribute("key", "description"), new XAttribute("value", "Description")));


                foreach (var e in trace.events)
                {
                    if (e.Value.Contains("init_"))
                    {
                        continue;
                    }
                    dt = dt.AddMilliseconds(1);
                    XElement evt = new XElement("event",
                        new XElement("string", new XAttribute("key", "concept:name"), new XAttribute("value", e.Value)),
                        new XElement("string", new XAttribute("key", "time:timestamp"), new XAttribute("value", dt.ToUniversalTime())),
                        new XElement("string", new XAttribute("key", "lifecycle:transition"), new XAttribute("value", "complete")));
                    trc.Add(evt);
                }
                root.Add(trc);
            }

            root.Save(Environment.CurrentDirectory + @"\traces.xml");
        }
    }
}
