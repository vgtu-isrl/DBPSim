using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using BBNGs.Graph;
using BBNGs.TraceLog;
using BBNGs.BBNs;
using BBNGs.VariableGenerators;

namespace BBNGs.Engine
{
    [Serializable]
    public class BBNGine
    {

        static string log = Environment.CurrentDirectory + @"\log.txt";
        static string log2 = Environment.CurrentDirectory + @"\log2.csv";

        string xsNs =  "http://www.xes-standard.org/";
        XNamespace xes
        {
            get{
                return xsNs;
            }
        }
        public List<Trace> traces { get { return TrainingSet; } }

        private List<Trace> TrainingSet = new List<Trace>();
        public List<Trace> TestSet = new List<Trace>();

        public Tree traceTree;
        public CPTG probs;
        internal  BBNGenerator bbn;

        [NonSerialized]
        public XDocument traceXesDocument;

        SquareMatrix frequencyMatrix;
        SquareMatrix setFrequencyMatrix;
        SquareMatrix mustFollowMatrix;
        SquareMatrix orMatrix;
        SquareMatrix childParentMatrix;


        public BBNGine()
        {

        }

        public bool ExtractBBN()
        {
            if (traceTree == null)
                return false;

            bbn = new BBNGenerator(traceTree);
            return true;
        }

        public bool ExtractTree()
        {
            if (traces == null)
                return false;
            Console.WriteLine("Creating tree");
            traceTree = Tree.CreateTree(traces,frequencyMatrix,setFrequencyMatrix,mustFollowMatrix,orMatrix,childParentMatrix);
           //System.Diagnostics.Process proc = new System.Diagnostics.Process();
           ////proc.StartInfo.FileName = @"C:\Program Files (x86)\Graphviz2.38\bin\dotty.exe";
           //proc.StartInfo.Arguments = traceTree.OutputGraphs()[0];
           //proc.Start();

          


            return true;
        }

        public bool LoadXesDocument(string filename, bool createTrainingSet)
        {
            List<string> attritubtesToIgnore = new List<string>() { };// "org:resource" };//,"action_code","activityNameNL", "dateFinished", "dueDate","planned"};
            List<string> eventToIgnore = new List<string>() { };// "s_k_i_r_i_u__stipendiją" };
            Console.WriteLine("Extracting Traces");
            traceXesDocument = XDocument.Load(filename);
            //ClearDocument(traceXesDocument, xes);
            var traceElements = traceXesDocument.Root.Elements(xes + "trace");
            List<Trace> traces = new List<Trace>();
            int i = 0;

            traceElements.AsParallel().ForAll(trace =>
                {
                    i++;
                    if (i % 500 == 0)
                        Console.WriteLine(i);
                    Trace t = new Trace(trace, xes, attritubtesToIgnore, eventToIgnore);

                    lock (traces)
                    {
                        traces.Add(t);
                    }
                });
            //traces = traces.OrderBy(x => x.startTime).ToList();

            TrainingSet = new List<Trace>();
            if (createTrainingSet)
            {
                Random r = new Random(1);
                int am = (int)(traces.Count * 0.70);
                for (int k = 0; k < am; k++)
                {
                    int idx = r.Next(traces.Count);
                    Trace t = traces[idx];
                    traces.Remove(t);
                    TrainingSet.Add(t);
                }
                TestSet = traces;
            }
            else
            {
                TrainingSet = traces.ToList();
            }

            //make frequency matrix
            frequencyMatrix = GetFrequencyMatrix();
            frequencyMatrix.TryAddColumn("start_event");
            frequencyMatrix.TransferCountsAsData();
            setFrequencyMatrix = GetSetFrequencyMatrix();
            setFrequencyMatrix.TransferCountsAsData();
            setFrequencyMatrix.TryAddColumn("start_event");
            setFrequencyMatrix.TransferCountsAsData();


            mustFollowMatrix = setFrequencyMatrix.CopyEmpty();
            orMatrix = setFrequencyMatrix.CopyEmpty();
            childParentMatrix = setFrequencyMatrix.CopyEmpty();


            MakeRuleMatrixes(frequencyMatrix,setFrequencyMatrix, mustFollowMatrix, orMatrix, childParentMatrix,traces.Count);

            Dictionary<string, SquareMatrix> matrixes = new Dictionary<string, SquareMatrix>();

            matrixes.Add("sequenceFrequency", frequencyMatrix);
            matrixes.Add("setFrequency", setFrequencyMatrix);
            matrixes.Add("mustFollow", mustFollowMatrix);
            matrixes.Add("orMatrix", orMatrix);
            matrixes.Add("childParent", childParentMatrix);

            Utility.OutputMatrix(matrixes,"matrix stats.csv");

            //traces.Select(x =>
            //    {
            //        var e = x.events.OrderBy(y => y.timestamp).ElementAt(1);
            //        if (e.name.ToLower().Contains("retract"))
            //        {
            //            return e.name + e.timestamp;
            //        }
            //        else return "";
            //    }).Distinct().ToList().ForEach(x=>
            //    {
            //        Console.WriteLine(x);
            //    });

           
                return true;
        }


        private static void MakeRuleMatrixes(SquareMatrix frequencyMatrix, SquareMatrix setFrequencyMatrix, SquareMatrix mustFollowMatrix, SquareMatrix orMatrix, SquareMatrix childParentMatrix, int traceCount)
        {
            foreach (var col in setFrequencyMatrix.columns.Values)
            {
                double cc = setFrequencyMatrix[col][col];
                //if (cc < setFrequencyMatrix.AlphaLevel)
                //   continue;
                foreach (var row in setFrequencyMatrix.rows.Values)
                {
                    if (col == row)
                        continue;
                    double rr = setFrequencyMatrix[row][row];
                    //if (rr < setFrequencyMatrix.AlphaLevel)
                    //  continue;

                    double cr = setFrequencyMatrix[col][row];
                    double rc = setFrequencyMatrix[row][col];
                    double scr = frequencyMatrix[col][row];
                    double src = frequencyMatrix[row][col];


                    if (cr==0 && rc==0)    //cr<cc*0.01 && rc<rr*0.01)
                    {
                        orMatrix.AddValue(col, row, 1);
                        orMatrix.AddValue(row, col, 1);
                    }
                    //if (cr > setFrequencyMatrix.AlphaLevel && rc < setFrequencyMatrix.AlphaLevel && scr > 0)
                    //    if (cr > setFrequencyMatrix.AlphaLevel && (rc < setFrequencyMatrix.AlphaLevel || src < frequencyMatrix.AlphaLevel) && scr > 0)
                    //if(cr>0 &&(rc<setFrequencyMatrix.AlphaLevel || src<frequencyMatrix.AlphaLevel) && scr>0)
                    

                    if ((rc == 0 && cr > 0) || (cr > 0 && rc < cr * 0.02))
                    {
                        mustFollowMatrix.AddValue(col, row, 1);
                    }

                    if (((cr > 0 && rc == 0) || (cr > setFrequencyMatrix.AlphaLevel && rc < cr * 0.1)) && scr > 0 && src < scr * 0.1)
                    {
                        childParentMatrix.AddValue(col, row, 1);
                    }
                }
            }
        }

        private SquareMatrix GetFrequencyMatrix()
        {
            try
            {
                SquareMatrix mtr = new SquareMatrix();
                foreach (var trace in traces)
                {
                    for (int j = 0; j < trace.events.Count - 1; j++)
                    {
                        string evName = trace.events[j].fullName;
                        mtr.TryAddColumn(evName);
                        string nevName = trace.events[j + 1].fullName;
                        mtr.TryAddColumn(nevName);
                        mtr.AddValue(evName, nevName, 0);
                    }

                }
                return mtr;
            }
            catch { return null; }
        }

       

        private SquareMatrix GetSetFrequencyMatrix()
        {
            try
            {
                SquareMatrix mtr = new SquareMatrix();
                foreach (var trace in traces)
                {
                    int idx = 0;
                    List<string> completedEvents = new List<string>();
                    foreach (Event ev in trace.events)
                    {
                        if (completedEvents.Contains(ev.fullName))
                        {
                            continue;
                        }
                        completedEvents.Add(ev.fullName);
                        mtr.TryAddColumn(ev.fullName);
                        List<string> toAdd = new List<string>();
                        for (int j = idx; j < trace.events.Count; j++)
                        {
                            string evName = trace.events[j].fullName;
                            toAdd.Add(evName);
                        }
                        foreach (var evName in toAdd)
                        {
                            mtr.TryAddColumn(evName);
                            mtr.AddValue(ev.fullName, evName, 0);
                        }
                        idx++;
                    }
                }
                return mtr;
            }
            catch { return null; }
        }

        //private void CleanUp(int limit)
        //{
        //    var events = (from trc in traces
        //                  from evt in trc.events
        //                  group trc by evt.fullName into x
        //                  select new
        //                  {
        //                      name = x.Key,
        //                      traceCt = x.ToList().Distinct().Count(),
        //                      events = (from z in x.ToList() from c in z.events where c.fullName == x.Key select new { trace = z, e = c }).Distinct().ToList()
        //                  }).ToList();



        //    int ctt = 0;



        //    for (int idx = 0; idx < events.Count; idx++)
        //    {
        //        var o = events[idx];
        //        if (o.traceCt < limit)
        //        {
        //            for (int idx2 = 0; idx2 < o.events.Count; idx2++)
        //            {
        //                ctt++;
        //                Event evt = o.events[idx2].e;
        //                evt.e.Remove();
        //                o.events[idx2].trace.events.Remove(evt);
        //                evt.trace = null;
        //            }
        //        }

        //    }
        //}

        private static void OutputEdges(Tree traceTree)
        {
            List<string> edges = traceTree.GetEdges();

            using (StreamWriter sw = new StreamWriter(File.Open(log2, FileMode.Truncate)))
            {
                foreach (string s in edges)
                    sw.WriteLine(s);

                foreach (List<string> loops in traceTree.loops)
                {
                    string l = "";
                    loops.ForEach(x => l += x + "=>");
                    sw.WriteLine(l.Remove(l.Length - 2));
                }
            }
        }

        private static void OutputDistinct(List<Trace> distinctTraces)
        {
            using (StreamWriter sw = new StreamWriter(File.Open(log, FileMode.Truncate)))
            {
                foreach (Trace t in distinctTraces)
                {
                    string eil = "=>\r\n";
                    foreach (Event e in t.events)
                        eil += "\t" + e.fullName + "\r\n";
                    sw.WriteLine(eil);
                    Console.WriteLine(eil);
                }
            }
        } 

        public void CreateCPT()
        {
            probs = new CPTG(traceTree, traces);
            probs.GenerateCPTs();
            probs.GenerateDependencies();
            
        }

        public void FilterLog(bool removeUncompleted, int maxTraceCount)
        {
            Console.WriteLine("Removing overhead");
            if (!File.Exists(Environment.CurrentDirectory + @"\1_edit.xes"))
                File.Create(Environment.CurrentDirectory + @"\1_edit.xes");

            while (traces.Count > maxTraceCount)
            {
                traces[0].trace.Remove();
                traces.RemoveAt(0);
            }

            List<string> eventsToRemove = new List<string>()
            {
                "determine_likelihood_of_claim",
                "b_check_if_sufficient_information_is_available",
                "s_check_if_sufficient_information_is_available"
            };

            if (removeUncompleted)
            {

                (from i in traces from j in i.events where j.fullName == "advise_claimant_on_reimbursementstart" select j).ToList().ForEach(x =>
                {
                    x.name = "check_on_advice";
                    x.transition = "complete";
                    x.fullName = "check_on_advicecomplete";

                    x.EventVals["concept:name"].evel.SetAttributeValue("value", "check on advice");
                    x.EventVals["lifecycle:transition"].evel.SetAttributeValue("value", "complete");

                });

                foreach (Trace t in traces)
                {
                    for (int i = 1; i < t.events.Count; i++)
                    {
                        if (t.events[i].transition != "complete" || eventsToRemove.Contains(t.events[i].name.ToLower()))
                        {
                            t.events[i].e.Remove();
                            t.events.RemoveAt(i);
                            t.EventTypes.Remove(t.events[i].fullName);
                            i--;
                        }
                    }
                }
            }



        }

       

        public List<string> ExtractDistinctEvents()
        {
            List<string> result = new List<string>();

            foreach (Trace t in traces)
            {
                foreach (string s in t.EventTypes.Keys)
                {
                    if (!result.Contains(s))
                        result.Add(s);
                }
            }

            return result;
        }

        public List<string> ExtractAttributes(string eventName)
        {

            List<Event> events = ExtractEvents(eventName);
            List<string> result = new List<string>();

            events.ForEach(x => x.EventVals.Keys.ToList().ForEach(y => result.Add(y)));

            return result.Distinct().ToList();
        }

        public List<Event> ExtractEvents(string eventName)
        {
            List<Event> events = new List<Event>();
            foreach (Trace t in this.traces)
            {
                Event val;
                t.TryToFind(eventName, out val);
                if (val != null)
                    events.Add(val);
            }
            return events;
        }

        public List<string> ExtractAttributeValues(string eventName, string attributeName)
        {
            List<Event> events = ExtractEvents(eventName);
            Dictionary<string, int> vals = new Dictionary<string, int>();

            foreach (Event e in events)
            {
                EventValue val;
                if (e.EventVals.TryGetValue(attributeName, out val))
                {
                    if (vals.ContainsKey(val.value))
                    {
                        vals[val.value]++;
                    }
                    else
                        vals.Add(val.value, 1);
                }
            }

            int sum = 0;
            foreach (int amount in vals.Values)
                sum += amount;

            List<string> result = new List<string>();
            foreach (string key in vals.Keys)
            {
                string st = key + "=" + (double)vals[key] / sum;
                result.Add(st);
            }

            return result;
        }

    }
}
