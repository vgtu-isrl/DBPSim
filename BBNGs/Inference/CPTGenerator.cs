using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BBNGs.Graph;
using BBNGs.TraceLog;
using BBNGs;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices; 

namespace BBNGs
{
    [Serializable]
    public class CPTG
    {
        Tree traceTree;
        public Dictionary<Node, NPROB> data
        { get; private set; }
        public Dictionary<Node, CPT> cpts
        { get; private set; }


        public CPT GetCPT(string name)
        {
            return cpts[traceTree.distinctNodes[name]];
        }
        public ErrorMargin GetErrorMargin(string name)
        {
            return margins[traceTree.distinctNodes[name]];
        }

        List<Trace> traces;
        
        public NodeAttributeDependency atb = new NodeAttributeDependency();

        public CPTG(Tree tree, List<Trace> traces)
        {
            this.traceTree = tree;
            this.traces = traces;
            data = new Dictionary<Node, NPROB>();
            cpts = new Dictionary<Node, CPT>();

            tree.distinctNodes.Values.ToList().ForEach(x => margins.Add(x, new ErrorMargin()));
        }
        
        public void GenerateCPTs()
        {
            traceTree.distinctNodes.Values.AsParallel().ForAll(n =>
            {
                Console.WriteLine(String.Format("Creating NPROB for {0}", n.name));
                NPROB p = new NPROB(n);
                lock (data)
                    data.Add(n, p);
            });
            traceTree.distinctNodes.Values.AsParallel().ForAll/*.ToList().ForEach*/(n =>
            {
                Console.WriteLine(String.Format("Creating CPT for {0}", n.name));
                CPT c = CPT.CreateCPT(n, data, null);//traces.Select(x => x.name));
                lock (cpts)
                    cpts.Add(n, c);
            });
            Console.WriteLine("Completed Generating CPTs");
        }
        
        private List<string> SystemObservables = new List<string>() { "occured", "No data", "previous" };

        private List<Tuple<string, string, string>> observations = new List<Tuple<string, string, string>>();

        public void ObserveVariable(string node, string column, string value)
        {
            Node n = traceTree.distinctNodes[node];
            observations.Add(new Tuple<string, string, string>(node, column, value));

            bool mustObserve = SystemObservables.Contains(column);
            bool observe =  true || mustObserve || n.parentNodes.Count ==0|| n.parentNodes.Select(x => deps[x][n].Contains(column)).Where(x => x==true).FirstOrDefault();

            if (!observe)
            {
                observe = false;
            }
            if (observe)
            {
                cpts[n].ObserveVariable(node, column, value);
                if (column == "occured")
                {
                   
                    if (!ActivatedNodes.Contains(n))
                    {
                        ActivatedNodes.Add(n);
                    }
                    else
                    {
                        ActivatedNodes.Remove(n);
                        ActivatedNodes.Add(n);
                    }

                }

                n.childNodes.ForEach(x =>
                {
                    if ( true || mustObserve || n.parentNodes.Count ==0|| n.parentNodes.Select(y => deps[y].ContainsKey(x) && deps[y][x].Contains(column)).Where(y => y==true).FirstOrDefault())
                    {
                        cpts[x].ObserveVariable(node, column, value);
                    }
                });
            }
        }

        public void ClearObservation(string node)
        {
            observations.Where(x => x.Item1 == node).ToList().ForEach(x => observations.Remove(x));
            cpts[traceTree.distinctNodes[node]].ClearObservations();
        }

        public void ClearPredictions()
        {
            ActivatedNodes = new List<Node>();
            badNodes = new HashSet<Node>();
            possibleChildNodes = new HashSet<Node>();
            observations = new List<Tuple<string, string, string>>();
            traceTree.distinctNodes.Keys.ToList().ForEach(x => ClearObservation(x));
            ActivatedNodes.Add(traceTree.distinctNodes["start_event"]);
            ClearObservation("start_event");
            ObserveVariable("start_event", "occured", "1");
        }

        
        public double GetProbability(string node)
        {
            return cpts[traceTree.distinctNodes[node]].ProbOfOccurence();
        }



        public ReplayResult ReplayTraceProbabilistically(Trace t)
        {
            ClearPredictions();
            Node startNode = null;
            


            for (int i = 0; i < t.events.Count - 1; i++)
            {
                Event e = t.events[i];
                if (e.fullName.Contains("endcomplete"))
                    continue;

                Node n = traceTree.distinctNodes[e.fullName];

                CPT cpt = cpts[n];

                double profOfOccurence = cpt.ProbOfOccurence();
                double standardChance = cpt.NonCausalProbOfOccurence();
                if (profOfOccurence < standardChance / 10)
                    return new ReplayResult().FailAsTooLowProbability(e, n, cpts);


                //foreach (var eVal in e.EventVals)
                //{
                //    ObserveVariable(n.name, eVal.Key, eVal.Value.value);
                //}

                double chance = cpts[n].ProbOfOccurence();
                double theoreticalChance = cpts[n].NonCausalProbOfOccurence();

                double totalChance = 1;
                //foreach(Node act in ActivatedNodes)
                //{
                //    if(!act.name.Contains("start_event") && !act.name.Contains("endcomplete"))
                //    {
                //        double ch = cpts[act].ProbOfOccurence();
                //        totalChance *= ch;
                //        theoreticalChance *= cpts[act].NonCausalProbOfOccurence();
                //    }
                //}

                if (totalChance < theoreticalChance * 0.01)
                    return new ReplayResult().FailAsTooLowTotalProbability(e, n, totalChance, theoreticalChance);


                ObserveVariable(n.name, "occured", "1");
                if (i == 0)
                {
                    if (!ActivatedNodes.Contains(n))
                        ActivatedNodes.Add(n);
                    continue;
                }

                var actNs = ActivatedNodes.Where(x => x.childNodes.Contains(n) || x == n);
                Node actN = null;
                foreach (Node aN in actNs)
                {
                    if (aN.name.StartsWith("start_event"))
                    {
                        continue;
                    }
                    if (traceTree.orMatrix.GetValue(aN.name, n.name) == 0)
                    {
                        actN = aN;
                        break;
                    }
                    else
                    {
                        return new ReplayResult().FailAsShouldNotOccurTogether(e, n, aN);
                    }

                }


                if (actN == null)
                {
                    return new ReplayResult().FailAsFailedToFindParent(e, n);
                }

                ActivatedNodes.Add(n);

            }

            return new ReplayResult().Succeed();
        }


       

        double min = 1;
        double max = 0;
        List<double> better = new List<double>();
        List<double> probas = new List<double>();
        double noData = 0;

        List<int> right = new List<int>();
        List<int> wrong = new List<int>();
        List<int> total = new List<int>();

        Dictionary<Node, ErrorMargin> margins = new Dictionary<Node, ErrorMargin>();

        List<Node> ActivatedNodes = new List<Node>();

        HashSet<Node> badNodes = new HashSet<Node>();
        HashSet<Node> possibleChildNodes = new HashSet<Node>();


        public Dictionary<Node, List<string>> correctVals = new Dictionary<Node, List<string>>();
        public PredictionResults TracePredictionStepByStep(Trace t)
        {
            #region prediction initialisation
            string eventTemplate = "Event:{0} has prob to occur {1}, with best next event {2}={3} and selection is correct={4}";
            //clear current predictions
            ClearPredictions();

            List<KeyValuePair<string, int>> result = new List<KeyValuePair<string, int>>();
            PredictionResults res = new PredictionResults();
            result.Add(new KeyValuePair<string, int>("Checking trace:" + string.Join(";", t.events.Select(x => x.name)), 0));


            #endregion
                       
            Dictionary<Node, double> guesses = new Dictionary<Node, double>();
            Dictionary<Node, double> pps = new Dictionary<Node, double>();
            Dictionary<Node, double> ppsn = new Dictionary<Node, double>();

            for (int i = 0; i < t.events.Count-2; i++)
            {
                Event e = t.events[i];
                if (!traceTree.distinctNodes.ContainsKey(e.fullName))
                {
                    continue;
                }

                //mark this event as activated
                Node n = traceTree.distinctNodes[e.fullName];

                //mark this node as observed
                CPT cpt = cpts[n];
                double chance = cpts[n].ProbOfOccurence();

                if (!pps.ContainsKey(n))
                {
                    pps.Add(n, cpt.InferWhatIfBasedOnOccured(null, null, null, true));
                    ppsn.Add(n, cpt.InferWhatIfBasedOnOccured(n.name, "occured", "-1", true));
                }
                foreach (var eVal in e.EventVals)
                {
                    ObserveVariable(n.name, eVal.Key, eVal.Value.value);
                }
                ObserveVariable(n.name, "occured", "1");

                ActivatedNodes.Add(n);

                //get all nodes that cannot occur together with this node
                IEnumerable<Node> possibleNodes;
                double bestCurrVal;
                Node bestNextChoice;
                MakePrediction(n, out possibleNodes, out bestCurrVal, out bestNextChoice);

                if (bestCurrVal == 1 && bestNextChoice.name == t.events[i + 1].fullName)
                {
                    var x = cpts[bestNextChoice].GetObservedValues();
                    List<string> lv = null;
                    if (!correctVals.TryGetValue(bestNextChoice, out lv))
                    {
                        lv = new List<string>();
                        correctVals.Add(bestNextChoice, lv);
                    }
                    lv.Add(x);
                }
                else
                {
                    if(t.events[i + 1].fullName == "initiate_paymentcomplete" && bestNextChoice.name != t.events[i + 1].fullName)
                    {
                        var tt = 0;
                    }
                }




                if (!traceTree.distinctNodes.ContainsKey(t.events[i + 1].fullName)) { continue; }
                double val = cpts[traceTree.distinctNodes[t.events[i + 1].fullName]].InferWhatIfBasedOnOccured(n.name, "previous", "1");
                if (double.IsNaN(val))
                {
                    val = cpts[traceTree.distinctNodes[t.events[i + 1].fullName]].ProbOfOccurence();
                    if (double.IsNaN(val))
                    {
                        val = cpts[traceTree.distinctNodes[t.events[i + 1].fullName]].NonCausalProbOfOccurence();
                    }

                }


                #region add statistical data
                double someBetter = 0;

                possibleNodes.ToList().ForEach(x =>
                {
                    double v = cpts[x].InferWhatIfBasedOnOccured(n.name, "previous", "1");
                    if (v > val && bestNextChoice != x)
                    {
                        someBetter += 1;
                    }
                });


                var margin = margins[traceTree.distinctNodes[t.events[i + 1].fullName]];

                if (!double.IsNaN(val) && !double.IsInfinity(val))
                {
                    if (margin.min > val) { margin.min = val; }
                    if (margin.max < val) { margin.max = val; }
                    if (min > val) { min = val; }
                    if (max < val) { max = val; }
                    margin.probas.Add(val);
                    probas.Add(val);

                }
                else
                {
                    margin.noData++;
                    noData++;
                }
                margin.better.Add(someBetter);
                better.Add(someBetter);
                #endregion

                #region add results
                bool correctChoice = false;
                if (bestNextChoice != null)
                {
                    if (bestNextChoice.name == t.events[i + 1].fullName)
                    {
                        correctChoice = true;
                    }
                    else
                    {
                        correctChoice = false;
                    }

                }


                bool partialCorrect = false;
                if (!correctChoice && bestNextChoice != null)
                {
                    int tidx = 0;
                    for (int j = 0; j < t.events.Count; j++)
                    {
                        if (t.events[j].fullName == bestNextChoice.name)
                        {
                            tidx = j;
                            break;
                        }
                    }
                    if (tidx != 0 && tidx > i)
                    {
                        partialCorrect = true;
                    }
                    //if (t.events.Where(x => x.fullName == bestNextChoice.name).FirstOrDefault() != null)
                    //{
                    //    partialCorrect = true;
                    //}
                }

                res.nodes.Add(n);
                Node nextNode = null;
                if (traceTree.distinctNodes.TryGetValue(t.events[i + 1].fullName, out nextNode))
                { }
                res.results.Add(new KeyValuePair<Node, PredictionResult>(n,
                    new PredictionResult()
                    {
                        bestNextChoice = bestNextChoice,
                        current = n,
                        next = nextNode,
                        realNextProb = nextNode == null ? 0 : cpts[nextNode].ProbOfOccurence(),
                        currentProb = cpts[n].ProbOfOccurence(),
                        nextProb = bestNextChoice != null ? cpts[bestNextChoice].ProbOfOccurence() : double.NaN,
                        correct = correctChoice,
                        failedPredictionWithOccurence = partialCorrect,
                        currentEvent = e,
                        nextEvent = t.events[i+1]
                    }));

                if (i > 0 && i < t.events.Count - 1)
                {
                    result.Add(
                        new KeyValuePair<string, int>(String.Format(eventTemplate, e.fullName, chance.ToString(), bestNextChoice == null ? "None" : bestNextChoice.name, bestCurrVal, correctChoice), correctChoice ? 1 : 0));
                }


                #endregion
            }

            //if (probas.Count % 100 == 0)
            //{
            //    double avg = probas.Sum() / probas.Count;
            //    double avgStdDev = Math.Sqrt(probas.Select(x => Math.Pow(x - avg, 2)).Sum() / probas.Count);
            //    double place = better.Sum() / better.Count;
            //    double placeStdDev = Math.Sqrt(better.Select(x => Math.Pow(x - avg, 2)).Sum() / better.Count);
            //    double c = probas.Count;
            //    Console.WriteLine(min + "(" + (probas.Where(x => x <= min).Count() / c).ToString("F1") + ") / " + max + "(" + (probas.Where(x => x >= max).Count() / c).ToString("F1") + ")/ " + avg.ToString("F3") + "\u2213" + avgStdDev.ToString("F3") + "(" + (probas.Where(x => x >= avg - avgStdDev && x < avg + avgStdDev).Count() / c).ToString("F1") + ") / " + place.ToString("F3") + "\u2213" + placeStdDev.ToString("F3") + " / " + ((double)noData / probas.Count * 100).ToString("F3") + "%");

            //}

            return res;// result;
        }

        
        public string GetMostProbableNextChoice()
        {
            var currEvent = "start_event";
            if (ActivatedNodes.Count > 0)
            {
                currEvent = ActivatedNodes[ActivatedNodes.Count - 1].name;
            }

            IEnumerable<Node> nodes;
            Node bestChoice;
            double bestCurrVal;

            MakePrediction(traceTree.distinctNodes[currEvent], out nodes, out bestCurrVal, out bestChoice);

            return bestChoice != null ? bestChoice.name : null;
        }

        public void MakePrediction(string nodeName, out IEnumerable<string> possibleNodes, out double bestCurrVal, out string bestNextChoice)
        {
            IEnumerable<Node> nodes;
            Node bestChoice;
            MakePrediction(traceTree.distinctNodes[nodeName], out nodes, out bestCurrVal, out bestChoice);

            possibleNodes = nodes.Select(x => x.name);
            bestNextChoice = bestChoice != null? bestChoice.name:null;
        }

        public void MakePrediction(Node n, out IEnumerable<Node> possibleNodes, out double bestCurrVal, out Node bestNextChoice)
        {
            if (n.name != "start_event")
            {
                traceTree.orMatrix.GetNonEmptyRowColumns(n.name, 0).ForEach(x =>
                {
                    if (!badNodes.Contains(traceTree.distinctNodes[x]))
                    {
                        badNodes.Add(traceTree.distinctNodes[x]);
                    }
                });

                var couldFollow = traceTree.setFrequencyMatrix.GetNonEmptyColumnRows(n.name, 0);
                traceTree.distinctNodes.Where(x => !couldFollow.Contains(x.Key) ).ToList().ForEach(x =>
                                  {
                                      if (x.Key == "start_event") { return; }

                                      if (!badNodes.Contains(traceTree.distinctNodes[x.Key]))
                                      {
                                          badNodes.Add(traceTree.distinctNodes[x.Key]);
                                      }
                                  });
            }




            //select possible further nodes 
            possibleNodes = (from k in ActivatedNodes
                             from j in k.childNodes
                             where
                                  traceTree.frequencyMatrix.GetValue(n.name, j.name) > 0
                                  //&& !ActivatedNodes.Contains(j)
                                  && !badNodes.Contains(j)
                             select j)/*.Where(x => !x.name.Contains("end"))*/.Distinct();
            if (possibleNodes.FirstOrDefault() == null)
            {
                possibleNodes = traceTree.distinctNodes.Where(x => x.Value.parentNodes.Count == 0 && !ActivatedNodes.Contains(x.Value)).Select(x => x.Value).Distinct();
            }
            var plock = new List<int>();

            Dictionary<Node, double> probs = new Dictionary<Node, double>();

            bestCurrVal = 0;
            bestNextChoice = null;
            double bestMaxCount = 0;
            if (n.name == "endcomplete" && possibleNodes.Count()>0) {
                int a = 5;
            }
            
            possibleNodes./*AsParallel().ForAll.*/ToList().ForEach(childNode =>
            {
                var cnCpt = cpts[childNode];

                double p = cpts[childNode].InferWhatIfBasedOnOccured(n.name, "previous", "1", false);// *(cpts[childNode].InferWhatIfBasedOnOccured(n.name, "previous", "1", true)/ cpts[childNode].InferWhatIfBasedOnOccured(n.name, "occured", "1", true));

                if (double.IsNaN(p))
                {
                    p = 0;
                    //var inp = cpts[childNode].ProbOfOccurence();
                    //if (double.IsNaN(inp))
                    //{
                    //    p = cpts[childNode].NonCausalProbOfOccurence();
                    //}
                    //else
                    //{
                    //    p = 0;
                    //}
                }
                else
                {
                }


                lock (plock)
                {
                    probs.Remove(childNode);
                    probs.Add(childNode, p);
                }
            });

            var maxProb = probs.Max(x => x.Value);
            foreach (var p in probs)
            {
                if(maxProb != p.Value)
                {
                    continue;
                }
                if (bestCurrVal < p.Value)
                {
                    var cnt = cpts[p.Key].InferWhatIfBasedOnOccured(n.name, "previous", "1", true);
                    bestCurrVal = p.Value;
                    bestNextChoice = p.Key;
                    bestMaxCount = cnt;
                }
                else if ( bestCurrVal == p.Value)
                {
                    var cnt = cpts[p.Key].InferWhatIfBasedOnOccured(n.name, "previous", "1", true);
                    if (cnt > bestMaxCount)
                    {
                        bestCurrVal = p.Value;
                        bestNextChoice = p.Key;
                        bestMaxCount = cnt;
                    }
                    else if (cnt == bestMaxCount && bestNextChoice != null)
                    {
                        var fst = traceTree.frequencyMatrix.GetValue(n.name, p.Key.name);
                        var snd = traceTree.frequencyMatrix.GetValue(n.name, bestNextChoice.name);
                        if(fst > snd)
                        {
                            bestCurrVal = p.Value;
                            bestNextChoice = p.Key;
                            bestMaxCount = cnt;
                        }
                        var tt = 0;
                    }
                    else
                    {
                        bestCurrVal = p.Value;
                        bestNextChoice = p.Key;
                        bestMaxCount = cnt;
                    }

                }
            }
        }

        public CPTGeneratedData GenerateData(string nodeName)
        {
            return cpts[traceTree.distinctNodes[nodeName]].GenerateEventData();
        }

        

        public Probability GetMaxProb(Node n)
        {
            NPROB prob = data[n];
            CPT cpt = cpts[n];

            var columns = prob.GetColumns();


            Probability p = new Probability();
            foreach (var col in columns)
            {
                ClearObservation(n.name);
                double maxOccur = 0;
                string maxOccurCol = string.Empty;
                string maxOccurVal = string.Empty;
                double minOccur = 1;
                string minOccurCol = string.Empty;
                string minOccurVal = string.Empty;
                IEnumerable<string> values = prob.values[col.Value].Keys;

                foreach (var val in values)
                {
                    cpt.ClearObservations();
                    cpt.ObserveVariable(n.name, col.Key, val);
                    double chance = cpt.ProbOfOccurence();
                    if (maxOccur < chance)
                    {
                        maxOccur = chance;
                        maxOccurCol = col.Key;
                        maxOccurVal = val;
                    }
                    if (minOccur > chance)
                    {
                        minOccur = chance;
                        minOccurCol = col.Key;
                        minOccurVal = val;
                    }
                }
                ValueProbability prMax = new ValueProbability() { parameter = col.Key, value = maxOccurVal, probability = maxOccur };
                ValueProbability prMin = new ValueProbability() { parameter = col.Key, value = minOccurVal, probability = minOccur };
                p.max.Add(col.Key, prMax);
                p.min.Add(col.Key, prMin);
            }
            return p;
        }


        private Dictionary<Node, Dictionary<Node, List<string>>> deps = new Dictionary<Node, Dictionary<Node, List<string>>>();

        internal void GenerateDependencies()
        {
            int i = 0;
            traceTree.distinctNodes.Values.AsParallel().ForAll(x =>
            {
                var atrs = x.GetAttributeList();
                var trcs = x.NodeObjects.Select(e => e.trace);

                Dictionary<Node, List<string>> depx = null;
                if(!deps.TryGetValue(x,out depx))
                {
                    depx = new Dictionary<Node, List<string>>();
                    deps.Add(x, depx);
                }
             

                x.childNodes.ForEach(y =>
                {
                    Console.Write("dep {0}=>{1}", x.name, y.name);
                    List<string> vtmp = null;
                    if(!depx.TryGetValue(y,out vtmp))
                    {
                        depx.Add(y, new List<string>());
                    }


                    var vals = new Dictionary<string, Dictionary<string, Tuple<int, int>>>();
                    atrs.ForEach(atr => vals.Add(atr, new Dictionary<string, Tuple<int, int>>()));
                    foreach (var trace in trcs)
                    {
                        var occ = trace.events.FirstOrDefault(e => e.fullName == y.name) != null;
                        var ev = trace.events.First(e => e.fullName == x.name);
                        foreach (var atr in atrs)
                        {
                            var atd = vals[atr];
                            EventValue v = null;
                            ev.EventVals.TryGetValue(atr, out v);
                            if (v == null)
                            {
                                continue;
                            }

                            Tuple<int, int> tup = null;
                            if (!atd.TryGetValue(v.value, out tup))
                            {
                                tup = occ ? new Tuple<int, int>(1, 0) : new Tuple<int, int>(0, 1);
                                atd.Add(v.value, tup);
                            }
                            else
                            {
                                if (occ)
                                {
                                    atd[v.value] = new Tuple<int, int>(tup.Item1+1, tup.Item2);
                                }
                                else
                                {
                                    atd[v.value] = new Tuple<int, int>(tup.Item1, tup.Item2+2);
                                }
                            }
                        }
                    }
                    foreach(var v in vals)
                    {
                        bool suits = false;
                        foreach(var vv in v.Value)
                        {
                            double proportion = (double)vv.Value.Item1 / vv.Value.Item2;
                            if(proportion<0.7 || proportion > 7)
                            {
                                suits = true;                           
                                break;
                            }
                        }
                        if (suits)
                        {
                            var atrnames = deps[x][y];
                            if (!atrnames.Contains(v.Key))
                            {
                                atrnames.Add(v.Key);
                            }
                        }
                    }
                });
            });
        }
    }

    public class CPTGeneratedData
    {
        public Dictionary<string, string> data;
        public double probability;
    }

    public class NodeAttributeDependency
    {
        public static Dictionary<string, List<KeyValuePair<string, Utility.DependenceFinder>>> GetDependencies(Node parentNode, Node childNode)
        {

            var natrs = parentNode.GetAttributeList();
            var traces = parentNode.traces.Union(childNode.traces).Distinct();// (from i in childNode.traces join j in parentNode.traces on i equals j select i);

            var atrs = childNode.GetAttributeList();

            Dictionary<string, List<string>> ndata = new Dictionary<string, List<string>>();
            natrs.ForEach(y => ndata.Add(y, new List<string>()));

            Dictionary<string, List<string>> xdata = new Dictionary<string, List<string>>();
            atrs.ForEach(y => xdata.Add(y, new List<string>()));

            foreach (var trace in traces)// traces.ForEach(trace =>
            {
                bool foundfirst = false;
                bool foundsecond = false;
                foreach (var ev in trace.events)
                {
                    if (!foundfirst && parentNode.name.StartsWith(ev.name))
                    {
                        foundfirst = true;
                        natrs.ForEach(y =>
                        {
                            lock (ndata[y])
                            {
                                if (ev.EventVals.ContainsKey(y))
                                {
                                    ndata[y].Add(ev.EventVals[y].value);
                                }
                                else
                                {

                                    ndata[y].Add("-1");
                                }
                            }
                        });
                    }

                    if (!foundsecond && childNode.name.StartsWith(ev.name))
                    {
                        foundsecond = true;
                        atrs.ForEach(y =>
                        {
                            lock (xdata[y])
                            {
                                if (ev.EventVals.ContainsKey(y))
                                {
                                    xdata[y].Add(ev.EventVals[y].value);
                                }
                                else
                                {
                                    xdata[y].Add("-1");
                                }
                            }
                        });
                    }
                }
                if (!foundfirst)
                {
                    natrs.ForEach(y => { lock (ndata[y]) { ndata[y].Add("-1"); } });
                }
                if (!foundsecond)
                {
                    atrs.ForEach(y => { lock (xdata[y]) { xdata[y].Add("-1"); } });
                }
            }//);


            Dictionary<string, List<KeyValuePair<string, Utility.DependenceFinder>>> dependencies = new Dictionary<string, List<KeyValuePair<string, Utility.DependenceFinder>>>();

            Parallel.ForEach<string>(natrs, new Action<string>((string natr) => {
                if (natr.Contains("time"))
                    return;
                List<string> data1 = ndata[natr];
                lock (dependencies)
                {
                    dependencies.Add(natr, new List<KeyValuePair<string, Utility.DependenceFinder>>());
                }
                foreach (string xatr in atrs)
                {
                    if (xatr.Contains("time"))
                        continue;
                    List<string> data2 = xdata[xatr];
                    var df = new Utility.DependenceFinder(data1, data2, data1.Distinct(), data2.Distinct());
                    dependencies[natr].Add(new KeyValuePair<string, Utility.DependenceFinder>(xatr, df));

                }
            }));

          
            return dependencies;
        }


        private Dictionary<Node, Dictionary<Node, AttributeDependency>> NodePair = new Dictionary<Node, Dictionary<Node, AttributeDependency>>();

        public bool GetValue(Node parentNode, Node childNode, string childAttribute)
        {
            Dictionary<Node, AttributeDependency> dep = null;
            if (NodePair.TryGetValue(parentNode, out dep))
            {
                AttributeDependency aDep = null;
                if (dep.TryGetValue(childNode, out aDep))
                {
                    return aDep.GetDependency(childAttribute);
                }
                else
                {
                    Console.WriteLine("Creating dependency");
                    aDep = AttributeDependency.CreateAttributeDependency(parentNode, childNode);
                    //Console.WriteLine(String.Format("Created dependency {0} v {1}", parentNode.name, childNode.name));
                    NodePair[parentNode].Add(childNode, aDep);
                    return aDep.GetDependency(childAttribute);
                }
            }
            else
            {
                Console.WriteLine("Creating dependency");
                var aDep = AttributeDependency.CreateAttributeDependency(parentNode, childNode);
                var pair = new Dictionary<Node, AttributeDependency>();
                pair.Add(childNode, aDep);
                NodePair.Add(parentNode, pair);
                return aDep.GetDependency(childAttribute);
            }

        }




        class AttributeDependency
        {
            Dictionary<string, Dictionary<string, bool>> dependencyValues = new Dictionary<string, Dictionary<string, bool>>();

            public static AttributeDependency CreateAttributeDependency(Node parentNode, Node childNode)
            {
                var atrDep = new AttributeDependency();
                var evDeps = GetDependencies(parentNode, childNode);
                foreach (var evDep in evDeps)
                {
                    foreach (var dp in evDep.Value)
                    {
                        if (dp.Value.significant)
                        {
                            atrDep.AddDependency(evDep.Key, dp.Key, dp.Value.significant);
                        }
                    }
                }
                return atrDep;
            }

            public void AddDependency(string attribute, string dependentAttribute, bool dependent)
            {
                Dictionary<string, bool> dependencies = null;
                if (dependencyValues.TryGetValue(attribute, out dependencies))
                {
                    if (dependencies.ContainsKey(dependentAttribute))
                    {
                        dependencies[dependentAttribute] = dependent;
                    }
                    else
                    {
                        dependencies.Add(dependentAttribute, dependent);
                    }
                }
                else
                {
                    dependencies = new Dictionary<string, bool>();
                    dependencies.Add(dependentAttribute, dependent);
                    dependencyValues.Add(attribute, dependencies);
                }
            }

            public bool GetDependency(string childAttribute)
            {
                if (childAttribute == null)
                {
                    return false;
                }
                bool result = false;
                Dictionary<string, bool> dependencies = null;
                if (dependencyValues.TryGetValue(childAttribute, out dependencies))
                {
                    result = dependencies.Where(x => x.Value).Select(x => x.Value).FirstOrDefault();
                }

                return result;
            }

            public bool GetDependency(string attribute, string dependentAttribute)
            {
                bool result = false;
                Dictionary<string, bool> dependencies = null;
                if (dependencyValues.TryGetValue(attribute, out dependencies))
                {
                    if (dependencies.ContainsKey(dependentAttribute))
                    {
                        result = dependencies[dependentAttribute];
                    }
                }

                return result;
            }
        }
    }

    public class PredictionResults
    {
        public List<Node> nodes = new List<Node>();
        public List<KeyValuePair<Node, PredictionResult>> results = new List<KeyValuePair<Node, PredictionResult>>();
    }

    public class PredictionResult
    {
        public Node current;
        public Node next;
        public Node bestNextChoice;
        public double nextProb;
        public double realNextProb;
        public double currentProb;
        public bool failedPredictionWithOccurence;
        public bool correct;
        public Event currentEvent;
        public Event nextEvent;
    }

    [Serializable]
    public class Probability
    {
        public Dictionary<string, ValueProbability> max = new Dictionary<string, ValueProbability>();
        public Dictionary<string, ValueProbability> min = new Dictionary<string, ValueProbability>();
    }

    [Serializable]
    public class ValueProbability
    {
        public string parameter;
        public string value;
        public double probability;

        public override string ToString()
        {
            return String.Format("P({0}={1})={2}", parameter, value, probability);
        }
    }

  

    [Serializable]
    public class ReplayResult
    {
        public bool result;
        public Event failingEvent;
        public string reason;

        public ReplayResult Succeed()
        {
            result = true;
            return this;
        }

        public ReplayResult FailAsFailedToFindParent(Event e, Node n)
        {
            string template = "Failed to find possible parent for node {0}. Expected any of:\r\n{1}";
            string nodeTemplate = "{0}\r\n";
            result = false;
            failingEvent = e;
            StringBuilder sb = new StringBuilder();
            if (n.parentNodes.Count > 0)
            {
                foreach (Node np in n.parentNodes)
                {
                    sb.Append(String.Format(nodeTemplate, np.name));
                }
            }
            else
                sb.Append("No identified parent nodes for the event");

            reason = String.Format(template, n.name, sb.ToString());
            return this;
        }

        public ReplayResult FailAsTooLowProbability(Event e, Node n, Dictionary<Node, CPT> cpts)
        {
            string template = "Probabilistic anomaly for node {0}. Chance of occurance is {1} while standard probability is {2}. Parents:\r\n";
            string nodeTemplate = "{0} has occured={1}\r\n";
            result = false;
            failingEvent = e;
            StringBuilder sb = new StringBuilder();


            foreach (Node np in n.parentNodes)
            {
                if (np.name.Contains("start_event") || np.name.Contains("endcomplete"))
                    continue;
                sb.Append(String.Format(nodeTemplate, np.name, cpts[np].Occured()));
            }

            reason = String.Format(template, n.name, cpts[n].ProbOfOccurence(), cpts[n].NonCausalProbOfOccurence()) + sb.ToString();
            return this;
        }

        public ReplayResult FailAsTooLowTotalProbability(Event e, Node n, double x, double y)
        {
            string template = "Probabilistic anomaly for node {0}. Standard chance of chain occurrence is {1} while standard probability is {2}.";
            result = false;
            failingEvent = e;
            reason = String.Format(template, n.name, x, y);
            return this;
        }

        public ReplayResult FailAsShouldNotOccurTogether(Event e, Node n, Node n2)
        {
            string template = "Event {0} should not occur together with {1}";
            result = false;
            failingEvent = e;
            reason = String.Format(template, n.name, n2.name);
            return this;
        }


    }

    public class ErrorMargin
    {
        public double min = 1;
        public double max = 0;
        public List<double> better = new List<double>();
        public List<double> probas = new List<double>();
        public double noData = 0;
        public List<int> right = new List<int>();
        public List<int> wrong = new List<int>();
        public List<int> total = new List<int>();


        private int astdc = 0;
        private double oastdc = 0;


        private int avc = 0;
        private double oavc = 0;

        public double avg { get {

            if (probas.Count-1 != avc)
            {
                while (probas.Count-1 > avc)
                {
                    oavc = (oavc * avc + probas[avc] / probas.Count);
                    avc++;
                }                
            }
            return oavc;
            //return probas.Sum() / probas.Count;
        
        } }
        public double avgStdDev { get {

            if (probas.Count-1 != astdc)
            {
                while (probas.Count-1 > astdc)
                {
                    oastdc = Math.Sqrt((Math.Pow(oastdc * avg, 2) + Math.Pow(probas[astdc] - avg, 2)) / astdc);
                    astdc++;
                }
            }
            return oastdc;
            //return Math.Sqrt(probas.Select(x => Math.Pow(x - avg, 2)).Sum() / probas.Count);
        
        
        } }
        public double place { get { return better.Sum() / better.Count; } }
        public double placeStdDev { get { return Math.Sqrt(better.Select(x => Math.Pow(x - avg, 2)).Sum() / better.Count); } }
        public double c { get { return probas.Count; } }
        public double lessThanMinProc { get { return (probas.Where(x => x <= avg - avgStdDev).Count() / c); } }
        public double avaregeProc { get { return (probas.Where(x => x >= avg - avgStdDev && x <= avg + avgStdDev).Count() / c); } }
        public double moreThanAverageProc { get { return (probas.Where(x => x >= avg + avgStdDev).Count() / c); } }

    }


}
