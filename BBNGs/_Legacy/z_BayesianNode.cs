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
using BBNGs.Engine;
using BBNGs.Utilities;

namespace BBNGs.BBNs
{
    [Serializable]
    class BayesianNode
    {
        static Random r = new Random();
        public Range range { get; private set; }
        public Node treeNode { get; private set; }
        public VariableArray<int> values { get; private set; }
        public int[] orgData;
        private int[] existingData;
        public Dictionary<string, int> valueNames { get; private set; }
        public List<BayesianNode> parentNodes = new List<BayesianNode>();
        public string nodeName;
        public string shortName;
        public dynamic cpt { get; set; }
        public dynamic prior { get; set; }
        public dynamic posterior { get; set; }
        public string keyToExtract { get; set; }
        //   private Variable<bool> ignoreNoData = Variable.Observed<bool>(true);

        public BayesianNode(Node treeNode, string valuesToExtract, int[] data, Dictionary<string, int> vals)
        {
            treeNode.chosenExtractionAttribute = valuesToExtract;
            this.keyToExtract = valuesToExtract;
            this.treeNode = treeNode;
            this.nodeName = treeNode.name;
            if (nodeName.Length > 20)
            {
                shortName = nodeName.Remove(0, nodeName.Length - 20) + r.Next(0, 1000);
            }
            else
                this.shortName = nodeName;
            valueNames = vals;
            range = new Range(valueNames.Count);//.Named("R" + shortName+r.Next(0,20000));
            orgData = data;
        }

        public override string ToString()
        {
            return nodeName;
        }

        public void SetOrgValueData(Range dr)
        {
            // values.ObservedValue = orgData;
            //   return;

            //if (toSet)
            //{
            //    existingData = new int[orgData.Length];
            //    for (int i = 0; i < orgData.Length; i++)
            //        existingData[i] = orgData[i];
            //    VariableArray<int> dt = Variable.Observed(existingData, dr);
            //    using (Variable.ForEach(dr))
            //    {
            //        using (Variable.If(dt[dr] > 0))
            //        {
            //            values[dr] = dt[dr];
            //        }
            //    }
            //    toSet = false;

            //}
            //else
            //{
            //for (int i = 0; i < orgData.Length; i++)
            //    existingData[i] = orgData[i];
            values.ObservedValue = orgData;
            // }



            //VariableArray<int> dt = Variable.Observed(orgData, dr);
            //VariableArray<int> data = Variable.Array<int>(dr);
            //Variable<int> got = Variable.New<int>();
            //got.ObservedValue = 0;
            //using (Variable.ForEach(dr))
            //{
            //    using (Variable.If(dt[dr] > 0))
            //    {
            //        data[dr] = dt[dr];
            //    }
            //}
            //if (set)
            //    //values = data;
            //    values.ObservedValue = orgData;//.SetTo(data);
            //else
            //{
            //    Console.WriteLine(this.nodeName + "->" + got);
            //    //values = data;
            //    values = data;
            //    values.ObservedValue = orgData;
            //    set = true;
            //}



        }
        
        public void SetValueData(Range dr, int value)
        {
            //return;
            //VariableArray<int> dt = Variable.Observed(orgData, dr);
            //VariableArray<int> data = Variable.Array<int>(dr);
            //using (Variable.ForEach(dr))
            //{
            //    using (Variable.If(dt[dr] >0))
            //    {
            //        data[dr] = value;
            //    }
            //}
            //values.SetTo(data);
        }

        public void setCptValueRange()
        {
            this.cpt.SetValueRange(this.range);
        }

        public void CreateNodePrior(List<Range> ranges)
        {
            if (ranges.Count > 0)
                this.prior = AddArrayOfVectors<Dirichlet>(ranges);//.Named("Pr" + this.shortName);
            else
            {
                this.prior = Variable.New<Dirichlet>();//;.Named("Pr" + this.shortName);
            }
        }

        public void CreateNodeCPT(List<Range> ranges)
        {
            if (ranges.Count > 0)
                this.cpt = AddArrayOfVectors<Vector>(ranges);//.Named("C" + this.shortName);
            else
            {
                this.cpt = Variable<Vector>.Random(this.prior);//.Named("C" + this.shortName);
            }
            
        }
        
        public dynamic AddArrayOfVectors<T>(List<Range> ranges)
        {
            return AddVariableArray(ranges, 1, Variable.Array<T>(ranges[0]));
        }

        private dynamic AddVariableArray(List<Range> ranges, int idx, dynamic prior)
        {
            if (idx < ranges.Count)
                return AddVariableArray(ranges, idx + 1, Variable.Array(prior, ranges[idx]));
            else
                return prior;
        }

        public void CombinePriorAndCPT(List<Range> ranges)
        {
            AddCPTs(ranges, this.cpt, this.prior, ranges.Count - 1);
        }

        private void AddCPTs(List<Range> ranges, dynamic nodeprob, dynamic nodePrior, int idx)
        {
            if (idx > 0)
                AddCPTs(ranges, nodeprob[ranges[idx]], nodePrior[ranges[idx]], idx - 1);
            else
                nodeprob[ranges[0]] = Variable<Vector>.Random(nodePrior[ranges[0]]);
        }

        public void AddChildFromParents(
            List<BayesianNode> parents,
            Range mainRange,
            dynamic cpt)
        {
            existingData = new int[orgData.Length];
            for (int i = 0; i < orgData.Length; i++)
                existingData[i] = orgData[i];
            VariableArray<int> dt = Variable.Observed(existingData, mainRange);

            if (parents.Count == 0)
            {
                var child = Variable.Array<int>(mainRange).Named("B" + this.shortName);
                //child.SetValueRange(range);

                using (Variable.ForEach(mainRange))
                {
                    //  using (Variable.IfNot(ignoreNoData))
                    //  {
                    //     using (Variable.If(dt[mainRange] > 0))
                    //      {
                    child[mainRange] = Variable.Discrete(cpt);
                    //      }
                    //   }
                    //    using (Variable.If(ignoreNoData))
                    //    {
                    //         child[mainRange] = Variable.Discrete(cpt);
                    //     }
                }
                //child[mainRange] = Variable.Discrete(this.cpt).ForEach(mainRange);
                this.values = child;
            }
            else
            {
                var n = parents[0].values.Range;
                var child = Variable.Array<int>(n);//.Named("B" + this.shortName);
                //child.SetValueRange(range);
                AddChild(parents, child, n, parents.Count, cpt, dt);
                this.values = child;
            }
        }

        private void AddChild(List<BayesianNode> parents, VariableArray<int> child, Range n, int idx, dynamic cpt, VariableArray<int> dt)
        {



            if (idx >= parents.Count)
                using (Variable.ForEach(n))
                using (Variable.Switch(parents[parents.Count - 1].values[n]))
                    AddChild(parents, child, n, parents.Count - 2, cpt[parents[parents.Count - 1].values[n]], dt);
            else if (idx >= 0)
                using (Variable.Switch(parents[idx].values[n]))
                    AddChild(parents, child, n, idx - 1, cpt[parents[idx].values[n]], dt);
            else
            {
                //     using (Variable.IfNot(ignoreNoData))
                // {
                //   using (Variable.If(dt[n] > 0))
                //   {
                child[n] = Variable.Discrete(cpt);
                //   }
                //      }
                //     using(Variable.If(ignoreNoData))
                //      {
                //          child[n] = Variable.Discrete(cpt);
                //       }

            }



        }
        
        public void MakeUniformPrior()
        {
            List<Dictionary<string, int>> vals = new List<Dictionary<string, int>>();
            this.parentNodes.ForEach(x => vals.Add(x.valueNames));
            if (vals.Count > 0)
            {
                dynamic observedPrior = MakeUniformPrior(vals, -1/*vals.Count*/, null);
                this.prior.ObservedValue = observedPrior;
            }
            else
                this.prior.ObservedValue = Dirichlet.Uniform(this.valueNames.Count);
        }

        public void MakeUniformObservation()
        {

            List<Dictionary<string, int>> vals = new List<Dictionary<string, int>>();
            this.parentNodes.ForEach(x => vals.Add(x.valueNames));
            if (vals.Count > 0)
            {
                dynamic observedPrior = MakeUniformPrior(vals, -1/*vals.Count*/, null);
                this.prior.ObservedValue = observedPrior;
            }
            else
                this.prior.ObservedValue = Dirichlet.Uniform(this.valueNames.Count);
        }

        private dynamic MakeUniformPrior(List<Dictionary<string, int>> parentValues, int idx, dynamic prior)
        {

            if (idx < 0)
            {
                return MakeUniformPrior(parentValues, 0, Dirichlet.Uniform(this.valueNames.Count));
            }
            else if (idx < parentValues.Count)
            {
                var closeMethod = new Utility().GetType().GetMethod("ReturnList", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(prior.GetType());
                dynamic list = closeMethod.Invoke(this, null);

                for (int i = 0; i < parentValues[idx].Values.Count; i++)
                    list.Add(prior);

                return MakeUniformPrior(parentValues, idx + 1, list.ToArray());
            }
            else
                return prior;
        }
    }
}
