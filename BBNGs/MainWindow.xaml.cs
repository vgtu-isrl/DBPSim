using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BBNGs.BBNs;
using BBNGs.Engine;
using BBNGs.TraceLog;
using BBNGs.Graph;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace BBNGs
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        ConsoleRedirectWriter rw = new ConsoleRedirectWriter();
        BBNGine engine = new BBNGine();
        bool logLoaded = false;
        bool treeLoaded = false;

        public MainWindow()
        {
            InitializeComponent();

            ConsoleRedirectWriter rw = new ConsoleRedirectWriter();
            rw.OnWrite += delegate(string value)
            {
                Dispatcher.Invoke((Action<string>)delegate(string val)
                {
                    if (OutputStreamBox.Text.Length > 10000)
                        OutputStreamBox.Clear();
                    OutputStreamBox.AppendText(val); OutputStreamBox.ScrollToEnd();
                }, System.Windows.Threading.DispatcherPriority.Normal, value);
            };

            LogFileTbx.Text = Environment.CurrentDirectory + @"\BPI_Challenge_2012.xes"; //;"\vgtu\1346994030.xes";
            LogFileTbx.Text = Environment.CurrentDirectory + @"\BPIC15_5.xes";
            LogFileTbx.Text = Environment.CurrentDirectory + @"\1_edit_old1.xes";



        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            rw.Release();
        }

        private void ExtractBtn_Click(object sender, RoutedEventArgs e)
        {
            new Task(
             new Action<object>(delegate(object filename)
             {


                 engine.LoadXesDocument((string)filename,true);
                 if (engine.traces != null)
                     if (engine.traces.Count > 0)
                     {
                         Dispatcher.Invoke(new Action(delegate() { ExtractTreeBtn.IsEnabled = true; }));
                     }

                 engine.ExtractDistinctEvents();
                 logLoaded = true;

             }), LogFileTbx.Text).Start();
        }

      
     

      
        private void SaveLogBtn_Click(object sender, RoutedEventArgs e)
        {
            if (engine.traceXesDocument != null)
                engine.traceXesDocument.Save(Environment.CurrentDirectory + @"\1_edit_edit.xes");
        }

        private void ExtractTreeBtn_Click(object sender, RoutedEventArgs e)
        {

            new Task(
             new Action(delegate()
             {
                 if (engine.ExtractTree())
                 {
                     Dispatcher.Invoke(new Action(delegate() { OutputGraphBtn.IsEnabled = true; }));

                     Dispatcher.Invoke(new Action(delegate()
                     {
                         BayesianNodesList.Items.Clear();
                         foreach (Node node in engine.traceTree.distinctNodes.Values.OrderBy(x => x.name))
                             BayesianNodesList.Items.Add(node);
                     }));
                     treeLoaded = true;
                     GenerateProbsBTn_Click(GenerateProbsBTn, e);
                 }

             })).Start();
        }

        private void ExtractBBNBtn_Click(object sender, RoutedEventArgs e)
        {
            new Task(
             new Action(delegate()
             {
                 bool succeeded = engine.ExtractBBN();

                
             })).Start();

        }

        private void OutputGraphBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = @"C:\Program Files (x86)\Graphviz2.38\bin\dotty.exe";
            proc.StartInfo.Arguments = engine.traceTree.OutputGraphs()[0];
            proc.Start();
        }

        private void CleanUpTree()
        {
            List<Node> nodes = engine.traceTree.distinctNodes.Values.ToList();

            for (int i = 0; i < nodes.Count; i++)
            {
                Node n = nodes[i];
                if (n.name.Contains("start_event"))
                    continue;
                List<Node> removable = new List<Node>();
                n.childNodes.ForEach(x =>
                {

                    if (n.name.Contains("start_event"))
                        return;
                    int kiekis = (from j in x.traces.AsParallel() join k in n.traces.AsParallel() on j equals k select j).Where(z => z.IsSequential(n.name, x.name)).Count();

                    if (kiekis > 10)
                        kiekis = kiekis;
                    if (kiekis < (int)(n.traces.Count / n.childNodes.Count * 0.3))
                        removable.Add(x);
                });

                for (int j = 0; j < removable.Count; j++)
                {
                    n.RemoveChildParentRelationship(removable[j]);
                }

                //foreach (Node nn in removable)
                //    n.RemoveChildParentRelationship(nn);
            }
        }

        

       

   
       

      

        private void ShowLoopsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (engine == null)
                return;

            engine.traceTree.loops.ForEach(x =>
            {
                Console.WriteLine("========");
                x.ForEach(y =>
                {
                    Console.WriteLine(y);
                });
            });
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"\d+$");
            List<List<string>> paths = new List<List<string>>();

            foreach (Trace t in engine.traces)
            {
                List<string> path = new List<string>();
                foreach (Event ev in t.events)
                {
                    path.Add(r.Replace(ev.name, ""));
                }
                paths.Add(path.Distinct().ToList());
            }

            List<List<string>> distinctPaths = new List<List<string>>();

            for (int i = 0; i < paths.Count; i++)
            {
                List<string> currPath = paths[i];
                bool found = false;
                for (int j = 0; j < distinctPaths.Count; j++)
                {
                    List<string> testPath = distinctPaths[j];

                    if (currPath.Count != testPath.Count)
                    {
                        continue;
                    }
                    bool checkedPathIsSame = true;
                    for (int k = 0; k < currPath.Count; k++)
                    {
                        if (currPath[k] != testPath[k])
                        {
                            checkedPathIsSame = false;
                            break;
                        }
                    }
                    if (checkedPathIsSame)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    distinctPaths.Add(currPath);
            }


            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(Environment.CurrentDirectory + @"\paths.txt"))
            {
                List<string> output = new List<string>();
                distinctPaths.ForEach(x =>
                {
                    string p = "";
                    x.ForEach(y =>
                    {
                        p += y + ",";
                        //p += (y.Length > 5) ? y.Remove(5)+"," : y+",";
                    });
                    output.Add(p);
                });

                output.OrderBy(x => x).ToList().ForEach(x => sw.WriteLine(x));
            }
        }

        private void ExtractLoopsBtn_Click(object sender, RoutedEventArgs e)
        {
           

            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(Environment.CurrentDirectory + @"\loops.txt"))
            {
                List<List<string>> loops = engine.traceTree.FindLoops();

                foreach (List<string> loop in loops)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (string loopingNode in loop)
                        sb.Append(loopingNode + "=>");
                    sw.WriteLine(sb.ToString());
                    Console.WriteLine(sb.ToString());
                }
                foreach(Node n in engine.traceTree.distinctNodes.Values)
                {
                    if (n.CheckIfLoops(engine.traceTree))
                        Console.WriteLine(n.name);
                }

                engine.traceTree.loops.ForEach(x =>
                {
                    string p = "";
                    x.ForEach(y =>
                    {
                        p += y + ",";
                    });
                    sw.WriteLine(p);
                });
            }
            while (engine.traceTree.FindLoops().Count > 0)
            {
                Console.WriteLine("Found");
            }
        }
        
      

        private void BayesianNodesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AttributesList.Items.Clear();

            if (BayesianNodesList.SelectedItem != null)
            {
                Node n = BayesianNodesList.SelectedItem as Node;
                engine.traceTree.selectedNode = n;
                parentBox.Items.Clear();
                n.parentNodes.ForEach(x => parentBox.Items.Add(x.name));
                if (engine.bbn == null)
                {
                    AttributesList.Items.Add("No data");
                    AttributesList.Items.Add("occured");
                    n.GetAttributeList().ForEach(x => AttributesList.Items.Add(x));
                }
                else
                    if (!string.IsNullOrWhiteSpace(n.chosenExtractionAttribute))
                    {
                        AttributesList.Items.Add(n.chosenExtractionAttribute);
                        SelectedAttrLbl.Content = n.chosenExtractionAttribute + n.traces.Count;
                    }
                    else
                        SelectedAttrLbl.Content = "Random" + n.traces.Count; ;

                if (engine.bbn != null)
                {
                    BayesianNode selectedNode = null;
                    if (engine.bbn.modelNodes.TryGetValue(n.name, out selectedNode))
                        engine.bbn.selectedNode = selectedNode;
                }
            }
        }

        private void AttributesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ValueList.Items.Clear();

            if (BayesianNodesList.SelectedItem != null && AttributesList.SelectedItem != null)
            {

                Node n = BayesianNodesList.SelectedItem as Node;
                string atr = AttributesList.SelectedItem as string;
                if (engine.bbn == null)
                    n.chosenExtractionAttribute = atr;
                SelectedAttrLbl.Content = n.chosenExtractionAttribute;

                (from i in n.NodeObjects select i.Find(atr)).Distinct().ToList().ForEach(x => ValueList.Items.Add(x));

            }
        }

        private void ValueList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BayesianNodesList.SelectedItem != null && AttributesList.SelectedItem != null && ValueList.SelectedItem != null)
            {
                Node n = BayesianNodesList.SelectedItem as Node;
                string atrVal = ValueList.SelectedItem as string;
                n.chosenExtractionAttributeValue = atrVal;
            }
        }

     

        private void BayesianNodesList_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Delete)
            {
                if (BayesianNodesList.SelectedItems.Count > 0)
                {
                    for (int i = 0; i < BayesianNodesList.SelectedItems.Count; i++)
                    {
                        Node n = BayesianNodesList.SelectedItems[i] as Node;

                        while (n.parentNodes.Count > 0)
                            n.parentNodes[0].SkipNodeAsChild(n);
                        while (n.childNodes.Count > 0)
                            n.childNodes[0].SkipNodeAsParent(n);
                        engine.traceTree.distinctNodes.Remove(n.name);
                    }
                }


            }
        }

        private void ShowDependencies(object sender, RoutedEventArgs e)
        {
            if (BayesianNodesList.SelectedItem != null)
            {
                Node n = BayesianNodesList.SelectedItems[0] as Node;

                List<Node> childNodes = new List<Node>();
                childNodes.Add(BayesianNodesList.SelectedItems[1] as Node);


                foreach (Node node in childNodes)
                {
                    Dictionary<string, List<KeyValuePair<string, Utility.DependenceFinder>>> dependencies = NodeAttributeDependency.GetDependencies(n, node);

                    foreach(var ndep in dependencies)
                    {
                        foreach(var xdep in ndep.Value)
                        {
                            if(xdep.Value.significant)
                            {
                                Console.WriteLine("!!!!P({0}|{1})", ndep.Key, xdep.Key);
                            }
                        }
                    }
                    Console.WriteLine("Completed dependency check.");
                }
            }
        }
        
        private void replayTracesBtn_Click(object sender, RoutedEventArgs e)
        {
            new Task(
                new Action(delegate()
            {
                int i = 0;
                int failed = 0;

                using (StreamWriter sw = new StreamWriter(File.Open(Environment.CurrentDirectory + @"\failedTraces.txt", FileMode.Create, FileAccess.ReadWrite)))
                {
                    foreach (Trace t in engine.traces)
                    {
                        if (i++ % 250 == 0)
                            Console.WriteLine("completed {0} with {1} failed", i, failed);
                        Event failedEvent = engine.traceTree.ReplayTrace(t);
                        if (failedEvent != null)
                        {
                            failed++;
                            StringBuilder sb = new StringBuilder();
                            t.events.ForEach(x => sb.Append( (x==failedEvent? "{"+x.name+"}":x.name )+ "->"));
                            sw.WriteLine(sb.ToString());
                            sw.WriteLine();
                        }

                    }
                    Console.WriteLine("Complete. Overall {0} replayed with {1} failed", i, failed);
                }
            })
                ).Start();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            string fn = Environment.CurrentDirectory + @"\data.dat";
                 if (File.Exists(fn))
                 {
                     if (MessageBox.Show("Do you want to load the data?", "Load", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                     {
                         Console.WriteLine("Loading previous data "+DateTime.Now.ToString());
                         new Task(
                             new Action(delegate()
                                 {
                                     using (FileStream fs = File.Open(fn, FileMode.Open, FileAccess.Read))
                                     {
                                         MemoryStream ms = new MemoryStream();
                                         fs.CopyTo(ms);
                                         ms.Position = 0;
                                         BinaryFormatter bf = new BinaryFormatter();
                                         engine = (BBNGine)bf.Deserialize(ms);
                                         Console.WriteLine("Log Loaded " + DateTime.Now.ToString());
                                     }
                                 })).Start();
                         
                     }
                     else
                     {
                         ExtractBtn_Click(sender, e);

                         new Task(
                          new Action(delegate()
                          {
                              while (!logLoaded)
                              {
                                  System.Threading.Thread.Sleep(150);
                              }

                              ExtractTreeBtn_Click(sender, e);
                          })).Start();
                     }  
                 }
                 else
                 {
                     ExtractBtn_Click(sender, e);

                     new Task(
                      new Action(delegate()
                      {
                          while (!logLoaded)
                          {
                              System.Threading.Thread.Sleep(150);
                          }

                          ExtractTreeBtn_Click(sender, e);
                      })).Start();
                 }           
        }


        private void ShowCPTBtn_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Exporing cpt");
            if (BayesianNodesList.SelectedItem == null)
                return;
            Node n = (Node)BayesianNodesList.SelectedItem;
            new Task(
               new Action(delegate()
               {
                   CPT p = engine.probs.cpts[n];
                   using (StreamWriter sw = new StreamWriter(File.Create(Environment.CurrentDirectory + @"\cpt.csv")))
                   {
                       sw.WriteLine(p.GetColumnRow());
                       foreach (string x in p.GetRow())
                       {
                           sw.WriteLine(x);
                       }
                   }
                   Console.WriteLine("Exportation completed.");

               })
               ).Start();                
        }

        private void ObserveBtn_Click(object sender, RoutedEventArgs e)
        {
            Node n = BayesianNodesList.SelectedItem as Node;
            string attribute = AttributesList.SelectedItem as string;
            string value = ValueList.SelectedItem as string;


            engine.probs.ObserveVariable(n.name, attribute, value);
            Console.WriteLine("Observation {0}->{1}={2} added",n.name,attribute,value);
        }

        private void ClearObservationBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach(Node n in engine.probs.cpts.Keys)
            {
                engine.probs.ClearObservation(n.name);
            }
            Console.WriteLine("Observations cleared.");
        }

        private void ProbChanceBtn_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Probability is {0}", engine.probs.GetProbability((BayesianNodesList.SelectedItem as Node).name));
        }

        private void ReplayProbBtn_Click(object sender, RoutedEventArgs e)
        {
            new Task(
               new Action(delegate()
               {

                   using (StreamWriter sw = new StreamWriter(File.Open(Environment.CurrentDirectory + @"\maxProbs.txt", FileMode.Create, FileAccess.ReadWrite)))
                   {
                       foreach (Node n in engine.traceTree.distinctNodes.Values)
                       {
                           Console.WriteLine("Inerring {0}", n.name);
                           Probability prop = engine.probs.GetMaxProb(n);
                           foreach (string k in prop.max.Keys)
                           {
                               string max = prop.max[k].ToString();
                               string min = prop.min[k].ToString();
                               sw.WriteLine("{0}; Min {1}; Max {2}", n.name, min, max);
                           }
                       }
                   }

                   int i = 0;
                   int failed = 0;

                   using (StreamWriter sw = new StreamWriter(File.Open(Environment.CurrentDirectory + @"\failedTraces_prob.txt", FileMode.Create, FileAccess.ReadWrite)))
                   {
                       for (int j = 0; j < engine.traces.Count; j++)
                           {
                               if (i++ % 250 == 0)
                                   Console.WriteLine("completed {0} with {1} failed", i, failed);
                               ReplayResult result = engine.probs.ReplayTraceProbabilistically(engine.traces[j]);
                               if (!result.result)
                               {
                                   failed++;
                                   StringBuilder sb = new StringBuilder();
                                   engine.traces[j].events.ForEach(x => sb.Append((x == result.failingEvent ? "{" + x.name + "}" : x.name) + "->"));
                                   sw.WriteLine(sb.ToString());
                                   sw.WriteLine(result.reason);
                                   sw.WriteLine("==========================================================================");
                                   sw.WriteLine();
                               }

                           }
                       Console.WriteLine("Complete. Overall {0} replayed with {1} failed", i, failed);
                   }
                  
               })
               ).Start();
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, engine);
            //var bytes = System.Text.Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(engine));
            //ms.Write(bytes,0,bytes.Length);    
            ms.Position = 0;
            using(FileStream fs = File.Open(Environment.CurrentDirectory+@"\data.dat",FileMode.Create,FileAccess.Write))
            {
                ms.CopyTo(fs);
            }
        }

        private void PredictBtn_Click(object sender, RoutedEventArgs e)
        {
            PredictionProgress prg = new PredictionProgress(engine);
            prg.Show();
            return;

            new Task(
               new Action(delegate()
               {
                   Dictionary<Node, bool> workers = new Dictionary<Node, bool>();

                   //engine.probs.GenerateDependencies();




                   List<Trace> trcs = engine.TestSet;

                   using (StreamWriter sw = new StreamWriter(File.Open(Environment.CurrentDirectory + @"\replay.txt", FileMode.Create, FileAccess.ReadWrite)))
                   {
                       int i = 0;

                       Dictionary<Node, KeyValuePair<int, int>> success = new Dictionary<Node, KeyValuePair<int, int>>();
                       foreach (Trace t in trcs)
                       {
                           sw.WriteLine("==========================================");
                           var res = engine.probs.TracePredictionStepByStep(t);

                           sw.WriteLine("Trace: "+string.Join(";", res.nodes.Select(x => x.name)));


                           int correct = 0;
                           int partialCorrect = 0;

                           foreach(var s  in res.results)
                           {
                               StringBuilder sb = new StringBuilder();
                               sb.Append(">" + s.Value.correct.ToString().Remove(1) + "#" + s.Value.failedPredictionWithOccurence.ToString().Remove(1) + "#");
                               sb.Append("P("+s.Value.current+")="+s.Value.currentProb);
                               sb.Append("#P(" + (s.Value.bestNextChoice == null ? "NaN" : s.Value.bestNextChoice.name)
                                   + ")="
                                   + s.Value.nextProb
                                   + "#" + (s.Value.next == null ? "NaN" : ("P("+s.Value.next.name+")="+s.Value.realNextProb)));
                               sw.WriteLine(sb.ToString());

                               if(s.Value.correct)
                               {
                                   correct++;
                               }
                               if(s.Value.failedPredictionWithOccurence)
                               {
                                   partialCorrect++;
                               }

                               KeyValuePair<int,int> r = default(KeyValuePair<int,int>);
                               if(success.TryGetValue(s.Key,out r))
                               {
                                   int a = r.Value + 1;
                                   int b = r.Key + (s.Value.correct ? 1 : 0);
                                   KeyValuePair<int, int> z = new KeyValuePair<int, int>(b,a);
                                   success[s.Key] = z;
                               }
                               else
                               {
                                   int a = 1;
                                   int b = s.Value.correct ? 1 : 0;
                                   KeyValuePair<int, int> z = new KeyValuePair<int, int>(b, a);
                                   success.Add(s.Key, z);
                               }
                           }

                           Node best = null;
                           double currBest = 0;
                           foreach(var x in success)
                           {
                               double v =(double)(x.Value.Key)/(double)(x.Value.Value);
                               if(v>currBest && x.Value.Value>10)
                               {
                                   currBest = v;
                                   best = x.Key;
                               }
                           }

                           if(best != null)
                           {
                               Console.WriteLine("Currbest:" + best.name + "->" + success[best].Key + "/" + success[best].Value + "  " + currBest);
                           }
                           //string bestString = "Best:"+best

                           
                           //foreach(var s in res)
                           //{
                           //    sw.WriteLine(s.Key);
                           //}
                           //int correct = 0;
                           //for (int j = 1; j < res.Count;j++)
                           //{
                           //    if(res[j].Value>0)
                           //    {
                           //        correct++;
                           //    }
                           //}



                           string resString = "Predicted:" + i.ToString() + " width " +partialCorrect+"#"+correct.ToString() + "#" + (res.results.Count - 1).ToString();
                           Console.WriteLine(resString);
                           i++;
                           sw.WriteLine(resString);
                           sw.WriteLine("===========================================");
                       }
                       foreach(var s in success)
                       {
                           sw.WriteLine(s.Key.name + "#" + s.Value.Key + "#" + s.Value.Value + "#" + ((double)s.Value.Key) / ((double)s.Value.Value));
                       }
                   }
               })
               ).Start();

        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            GraphPainter grp = new GraphPainter(engine.traceTree);
            grp.Show();
        }

        private void GenerateProbsBTn_Click(object sender, RoutedEventArgs e)
        {
            new Task(
               new Action(delegate()
               {
                   engine.CreateCPT();
                   Console.WriteLine("Complete");

               })
               ).Start();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            OutputStreamBox.Text = "";
            engine.probs.ClearPredictions();
            Node node = BayesianNodesList.SelectedItem != null ? (Node)BayesianNodesList.SelectedItem : null;
            var generatedData = this.engine.probs.GenerateData(node.name);

            Console.WriteLine(generatedData.probability);
            foreach (var x in generatedData.data)
            {
                Console.WriteLine(x.Key + ":" + x.Value);
            }

            engine.probs.ObserveVariable(node.name, "occured", "1");
            generatedData = this.engine.probs.GenerateData(node.name);

            Console.WriteLine(generatedData.probability);
            foreach (var x in generatedData.data)
            {
                Console.WriteLine(x.Key + ":" + x.Value);
            }

        }
    }
}
