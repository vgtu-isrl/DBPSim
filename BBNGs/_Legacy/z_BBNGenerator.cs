using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicrosoftResearch.Infer;
using MicrosoftResearch.Infer.Models;
using MicrosoftResearch.Infer.Distributions;
using MicrosoftResearch.Infer.Maths;
using System.Reflection;
using BBNGs.Graph;
using BBNGs.TraceLog;
using BBNGs.BBNs;
using BBNGs.Engine;
using System.IO;

namespace BBNGs.BBNs
{


    [Serializable]
    internal class BBNGenerator
    {

        Tree tracetree;
        public Dictionary<string, BayesianNode> modelNodes = new Dictionary<string, BayesianNode>();
        Variable<int> totalTraceCount;
        Range N;
        public BayesianNode selectedNode;

        Dictionary<string, int[]> data;
        int traceCount;
        List<string> Path;

        public BBNGenerator(Tree tracetree)
        {
            this.tracetree = tracetree;
            CreateModel();
        }

        public InferenceEngine Engine = new InferenceEngine();

        private void CreateModel()
        {
            //Engine.ShowFactorGraph = true;
            Engine.Compiler.GeneratedSourceFolder = "BBN_Infer";
            Engine.Compiler.GenerateInMemory = false;
            Engine.ShowWarnings = true;
            Engine.Compiler.FreeMemory = false;
            Engine.Compiler.ShowProgress = true;
            //Engine.Compiler.IgnoreEqualObservedValuesForValueTypes = true;
            //Engine.Compiler.UnrollLoops = true;
            //Engine.Compiler.OptimiseInferenceCode = true;
            Engine.Compiler.UseParallelForLoops = true;
            Engine.ShowProgress = true;



            Path = tracetree.rootNode.GetAllChildren().Distinct().ToList();

            //nodes for each of activity
            Dictionary<string, Node> information = tracetree.GetTracesOfPath(Path);
            (from i in Path where !information.Keys.Contains(i) select i).ToList().ForEach(x => Path.Remove(x));



            //distinct traces for learning
            List<Trace> traces = (from i in information.Values from j in i.traces select j).Distinct().ToList();
            //while (traces.Count > 500)
            //    traces.RemoveAt(traces.Count - 1);


            data = new Dictionary<string, int[]>();
            Path.ForEach(x => data.Add(x, new int[traces.Count]));

            Dictionary<string, Dictionary<string, int>> valueNames = new Dictionary<string, Dictionary<string, int>>();
            Path.ForEach(x => valueNames.Add(x, information[x].GetValues()));



            //Engine.OptimiseForVariables = (from i in this.modelNodes.Values select i.cpt as IVariable).ToList();

            int kiekis = 0;
            int failures = 0;
            for (int i = 0; i < traces.Count; i++)
            {
                for (int j = 0; j < Path.Count; j++)
                {
                    Event val;
                    Trace t = traces[i];
                    string pth = Path[j];

                    if (t.TryToFind(pth, out val))
                    {
                        Node n = information[pth];
                        if (!n.traces.Contains(t))
                            continue;
                        string result;
                        if (val.TryToFindValue(n.chosenExtractionAttribute, out result))
                        {
                            Dictionary<string, int> vals = valueNames[pth];
                            int v;
                            if (vals.TryGetValue(result, out v))
                            {
                                kiekis++;
                                data[pth][i] = v;
                            }
                        }
                    }
                }
            }

            if (valueNames.Keys.Contains("start_event"))
            {
                List<Trace> startTraces = information["start_event"].traces;
                for (int i = 0; i < traces.Count; i++)
                    startTraces.Add(traces[i]);
                information["start_event"].traces = startTraces;
                var startData = data["start_event"];
                for (int i = 0; i < startData.Length; i++)
                    startData[i] = 1;
            }

            //creates model based on path and information
            CreateModel(Path, information, data, valueNames);

            Console.WriteLine("kiekis=>"+kiekis);
            Console.WriteLine("failures=>"+failures);
            Console.WriteLine("traces=>"+traces.Count);
            traceCount = traces.Count;


        }
        
        public void InferId(int id)
        {
            LearnTheData(data, Path, traceCount, id, null);
        }

        public void Infer(BayesianNode bn)
        {
            LearnTheData(data, Path, traceCount, -5, bn);
        }

        public void MakeInfer()
        {
            LearnTheData(data, Path, traceCount, -5, selectedNode);
        }

        public void DisplayResult()
        {
            if (selectedNode != null)
            {
                this.totalTraceCount.ObservedValue = 1;
                Path.ForEach(x =>
                {
                    BayesianNode n = this.modelNodes[x];
                    if (n == selectedNode || String.IsNullOrWhiteSpace(n.treeNode.chosenExtractionAttributeValue))
                    {
                        n.values.ClearObservedValue();
                    }
                    else
                    {
                        n.values.ObservedValue = new int[] { n.valueNames[n.treeNode.chosenExtractionAttributeValue] };
                    }
                });

                selectedNode.treeNode.GetAllParents();

                dynamic o = GetPosteriorOfInferance(selectedNode);
                WriteAllElements(o, 0, selectedNode, null);
            }
        }
        
        internal void DisplayResult2()
        {
            if (selectedNode != null)
            {
                Path.ForEach(x =>
                {
                    BayesianNode n = this.modelNodes[x];



                    if (n == selectedNode || String.IsNullOrWhiteSpace(n.treeNode.chosenExtractionAttributeValue))
                    {
                        n.SetOrgValueData(N);
                    }
                    else
                    {
                        int observed = n.valueNames[n.treeNode.chosenExtractionAttributeValue];
                        n.SetValueData(N, observed);

                    }
                });


                dynamic o = GetPosteriorOfInferance(selectedNode);
                WriteAllElements(o, 0, selectedNode, null);
            }
        }
        
        private void LearnTheData(Dictionary<string, int[]> data, List<string> Path, int traceCount, int modelProb, BayesianNode chosen)
        {


            Console.WriteLine("Learning the data");

            totalTraceCount.ObservedValue = traceCount;
            foreach (string s in Path)
            {
                BayesianNode n = modelNodes[s];
                //n.orgData = data[s];
                //n.SetOrgValueData(N);
                n.MakeUniformPrior();
                n.SetOrgValueData(N);
            }

            int l = 0;
            foreach (string s in Path)
            {
                Console.WriteLine("Inferring {0}", l);
                BayesianNode nod = modelNodes[s];
                EngineInferNode(nod);
                l++;
            }
            l = 0;
            foreach (string s in Path)
            {
                Console.Write("Prior {0}", l);
                BayesianNode nod = modelNodes[s];
                nod.prior/*.ObservedValue*/ = nod.posterior;
                l++;
            }



            // WriteAllElements(n.posterior, 0, n);

        }

        private void EngineInferNode(BayesianNode nod)
        {
            switch (nod.parentNodes.Count)
            {
                case 0:
                    nod.posterior = Engine.Infer<Dirichlet>((nod.cpt));
                    break;
                case 1:
                    nod.posterior = Engine.Infer<Dirichlet[]>(nod.cpt);
                    break;
                case 2:
                    nod.posterior = Engine.Infer<Dirichlet[][]>(nod.cpt);
                    break;
                case 3:
                    nod.posterior = Engine.Infer<Dirichlet[][][]>(nod.cpt);
                    break;
                case 4:
                    nod.posterior = Engine.Infer<Dirichlet[][][][]>(nod.cpt);
                    break;
                case 5:
                    nod.posterior = Engine.Infer<Dirichlet[][][][][]>(nod.cpt);
                    break;
                case 6:
                    nod.posterior = Engine.Infer<Dirichlet[][][][][][]>(nod.cpt);
                    break;
                case 7:
                    nod.posterior = Engine.Infer<Dirichlet[][][][][][][]>(nod.cpt);
                    break;
                case 8:
                    nod.posterior = Engine.Infer<Dirichlet[][][][][][][][]>(nod.cpt);
                    break;
                case 9:
                    nod.posterior = Engine.Infer<Dirichlet[][][][][][][][][]>(nod.cpt);
                    break;
                case 10:
                    nod.posterior = Engine.Infer<Dirichlet[][][][][][][][][][]>(nod.cpt);
                    break;
                case 11:
                    nod.posterior = Engine.Infer<Dirichlet[][][][][][][][][][][]>(nod.cpt);
                    break;
                case 12:
                    nod.posterior = Engine.Infer<Dirichlet[][][][][][][][][][][][]>(nod.cpt);
                    break;
                case 13:
                    nod.posterior = Engine.Infer<Dirichlet[][][][][][][][][][][][][]>(nod.cpt);
                    break;
                case 14:
                    nod.posterior = Engine.Infer<Dirichlet[][][][][][][][][][][][][][]>(nod.cpt);
                    break;
                case 15:
                    nod.posterior = Engine.Infer<Dirichlet[][][][][][][][][][][][][][][]>(nod.cpt);
                    break;
                case 16:
                    nod.posterior = Engine.Infer<Dirichlet[][][][][][][][][][][][][][][][]>(nod.cpt);
                    break;
                case 17:
                    nod.posterior = Engine.Infer<Dirichlet[][][][][][][][][][][][][][][][][]>(nod.cpt);
                    break;
                case 18:
                    nod.posterior = Engine.Infer<Dirichlet[][][][][][][][][][][][][][][][][][]>(nod.cpt);
                    break;
                case 19:
                    nod.posterior = Engine.Infer<Dirichlet[][][][][][][][][][][][][][][][][][][]>(nod.cpt);
                    break;
                case 20:
                    nod.posterior = Engine.Infer<Dirichlet[][][][][][][][][][][][][][][][][][][][]>(nod.cpt);
                    break;
                case 21:
                    nod.posterior = Engine.Infer<Dirichlet[][][][][][][][][][][][][][][][][][][][][]>(nod.cpt);
                    break;
                case 22:
                    nod.posterior = Engine.Infer<Dirichlet[][][][][][][][][][][][][][][][][][][][][][]>(nod.cpt);
                    break;
                case 23:
                    nod.posterior = Engine.Infer<Dirichlet[][][][][][][][][][][][][][][][][][][][][][][]>(nod.cpt);
                    break;
                case 24:
                    nod.posterior = Engine.Infer<Dirichlet[][][][][][][][][][][][][][][][][][][][][][][][]>(nod.cpt);
                    break;
                case 25:
                    nod.posterior = Engine.Infer<Dirichlet[][][][][][][][][][][][][][][][][][][][][][][][][]>(nod.cpt);
                    break;
            }
        }

        private dynamic GetPosteriorOfInferance(BayesianNode nod)
        {


            dynamic z;
            switch (nod.parentNodes.Count)
            {
                case 0:
                    z = Engine.Infer<Dirichlet>((nod.cpt)); return z;

                case 1:
                    z = Engine.Infer<Dirichlet[]>(nod.cpt); return z;

                case 2:
                    z = Engine.Infer<Dirichlet[][]>(nod.cpt); return z;

                case 3:
                    z = Engine.Infer<Dirichlet[][][]>(nod.cpt); return z;

                case 4:
                    z = Engine.Infer<Dirichlet[][][][]>(nod.cpt); return z;

                case 5:
                    z = Engine.Infer<Dirichlet[][][][][]>(nod.cpt); return z;

                case 6:
                    z = Engine.Infer<Dirichlet[][][][][][]>(nod.cpt); return z;

                case 7:
                    z = Engine.Infer<Dirichlet[][][][][][][]>(nod.cpt); return z;

                case 8:
                    z = Engine.Infer<Dirichlet[][][][][][][][]>(nod.cpt); return z;

                case 9:
                    z = Engine.Infer<Dirichlet[][][][][][][][][]>(nod.cpt); return z;

                case 10:
                    z = Engine.Infer<Dirichlet[][][][][][][][][][]>(nod.cpt); return z;

                case 11:
                    z = Engine.Infer<Dirichlet[][][][][][][][][][][]>(nod.cpt); return z;

                case 12:
                    z = Engine.Infer<Dirichlet[][][][][][][][][][][][]>(nod.cpt); return z;

                case 13:
                    z = Engine.Infer<Dirichlet[][][][][][][][][][][][][]>(nod.cpt); return z;

                case 14:
                    z = Engine.Infer<Dirichlet[][][][][][][][][][][][][][]>(nod.cpt); return z;

                case 15:
                    z = Engine.Infer<Dirichlet[][][][][][][][][][][][][][][]>(nod.cpt); return z;

                case 16:
                    z = Engine.Infer<Dirichlet[][][][][][][][][][][][][][][][]>(nod.cpt); return z;

                case 17:
                    z = Engine.Infer<Dirichlet[][][][][][][][][][][][][][][][][]>(nod.cpt); return z;

                case 18:
                    z = Engine.Infer<Dirichlet[][][][][][][][][][][][][][][][][][]>(nod.cpt); return z;

                case 19:
                    z = Engine.Infer<Dirichlet[][][][][][][][][][][][][][][][][][][]>(nod.cpt); return z;

                case 20:
                    z = Engine.Infer<Dirichlet[][][][][][][][][][][][][][][][][][][][]>(nod.cpt); return z;

                case 21:
                    z = Engine.Infer<Dirichlet[][][][][][][][][][][][][][][][][][][][][]>(nod.cpt); return z;

                case 22:
                    z = Engine.Infer<Dirichlet[][][][][][][][][][][][][][][][][][][][][][]>(nod.cpt); return z;

                case 23:
                    z = Engine.Infer<Dirichlet[][][][][][][][][][][][][][][][][][][][][][][]>(nod.cpt); return z;

                case 24:
                    z = Engine.Infer<Dirichlet[][][][][][][][][][][][][][][][][][][][][][][][]>(nod.cpt); return z;

                case 25:
                    z = Engine.Infer<Dirichlet[][][][][][][][][][][][][][][][][][][][][][][][][]>(nod.cpt); return z;

            }
            return null;
        }
        
        private void WriteAllElements(dynamic arr, int currIdx, BayesianNode n, string row)
        {
            string tab = "";
            for (int k = 0; k < currIdx; k++)
                if (row != null)
                    tab += "\t";

            string prefix = row == null ? "" : row;
            if (currIdx < n.parentNodes.Count)
            {
                BayesianNode node = n.parentNodes[n.parentNodes.Count - currIdx - 1];
                string nodeName = node.nodeName;

                if (!string.IsNullOrWhiteSpace(node.treeNode.chosenExtractionAttributeValue))
                {
                    prefix += "," + nodeName + "=" + node.treeNode.chosenExtractionAttributeValue;
                    Console.WriteLine(tab + node.treeNode.chosenExtractionAttributeValue + "=" + nodeName);
                    WriteAllElements(arr[node.valueNames[node.treeNode.chosenExtractionAttributeValue]], currIdx + 1, n, prefix);
                }
                else
                    for (int i = 0; i < n.parentNodes[n.parentNodes.Count - currIdx - 1].valueNames.Count; i++)
                    {

                        Console.WriteLine(tab + n.parentNodes[n.parentNodes.Count - currIdx - 1].valueNames.ElementAt(i).Key + "=" + nodeName);
                        WriteAllElements(arr[i], currIdx + 1, n, prefix + "," + n.parentNodes[n.parentNodes.Count - currIdx - 1].valueNames.ElementAt(i).Key + "=" + nodeName);
                    }
            }
            else
            {
                var val = arr.GetMean();
                for (int j = 0; j < val.Count; j++)
                {
                    prefix += ";" + n.valueNames.ElementAt(j).Key + "=" + val[j].ToString();

                    Console.WriteLine(tab + n.valueNames.ElementAt(j).Key + "=" + val[j].ToString());
                }

                StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\problog.txt", true);
                sw.WriteLine(prefix);
                sw.Close();
            }
        }

        private dynamic GetArrayElementAt(dynamic arr, int[] idx, int currIdx)
        {
            if (currIdx < idx.Length - 1)
                return GetArrayElementAt(arr[idx[currIdx]], idx, currIdx + 1);
            else
                return arr[idx[currIdx]].GetMean()[idx[currIdx++]];
        }

        private void CreateModel(List<string> Path, Dictionary<string, Node> information, Dictionary<string, int[]> data, Dictionary<string, Dictionary<string, int>> valueNames)
        {
            totalTraceCount = Variable.New<int>();//.Named("NoOfTraces");
            N = new Range(totalTraceCount);//.Named("N");



            for (int i = Path.Count - 1; i > 0; i--)
            {
                CreateBayesianNode(Path, information, information[Path[i]], N, data, valueNames);
            }

        }
        
        private void CreateBayesianNode(List<string> Path, Dictionary<string, Node> information, Node treeNode, Range N, Dictionary<string, int[]> data, Dictionary<string, Dictionary<string, int>> valueNames)
        {
            if (modelNodes.Keys.Contains(treeNode.name))
                return;

            //"lytis","konkBal"};//"org:resource"};
            //create node
            BayesianNode node = null;

            node = new BayesianNode(treeNode, treeNode.chosenExtractionAttribute, data[treeNode.name], valueNames[treeNode.name]);

            //get direct parent nodes that are listed in path
            List<string> parents = node.treeNode.GetFilteredParents(Path).Distinct().OrderBy(x => Path.IndexOf(x)).ToList();


            //create parentNodes for the network
            (from j in parents where !modelNodes.Keys.Contains(j) select j).ToList().ForEach(nodeToCreate =>
            {
                CreateBayesianNode(Path, information, information[nodeToCreate], N, data, valueNames);
            });


            //get parent nodes
            List<BayesianNode> parentNodes = new List<BayesianNode>();
            parents.ForEach(x => parentNodes.Add(modelNodes[x]));

            //get parentRanges
            parentNodes.ForEach(x => node.parentNodes.Add(x));
            List<Range> ranges = new List<Range>();
            parentNodes.ForEach(x => ranges.Add(x.range));



            //create cpt table
            node.CreateNodePrior(ranges);
            node.CreateNodeCPT(ranges);

            if (ranges.Count > 0)
                node.CombinePriorAndCPT(ranges);

            node.setCptValueRange();


            node.AddChildFromParents(node.parentNodes, N, node.cpt);


            modelNodes.Add(node.nodeName, node);
        }




    }



}
