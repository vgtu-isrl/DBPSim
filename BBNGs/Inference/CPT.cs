using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BBNGs;
using BBNGs.Graph;
using BBNGs.TraceLog;
using BBNGs.Utilities;

namespace BBNGs
{
    [Serializable]
    public class CPT
    {
        private static  Random rnd = new Random(1);
        int[][] _aggregatedCPT = null;

        int[][] aggregatedCPT
        {
            get { return _aggregatedCPT; }
            set
            {
                _aggregatedCPT = value;
                int x = aggregatedCPT.Length > 0 ? aggregatedCPT[0].Length : 0;
                int idx = x > 0 ? x - 1 : 0;
                Total = 0;
                tree = NestedTree.CreateTree(aggregatedCPT);
                Total = tree.GetSum(new Dictionary<int, int>());
                ObservedTotal = tree.GetSum(new Dictionary<int, int>() { { idx - 1, 1 } });

            }
        }
        NestedTree tree = null;

        public int[][] CPTable
        {
            get { return aggregatedCPT; }
            protected set
            {
                aggregatedCPT = value;
            }
        }

        public class NestedTree
        {
            private int[,] arr;
            private long arrl;
            private long arrsi;
            private bool empty;

            public static NestedTree CreateTree(int[][] array)
            {
                return new NestedTree(array);

            }

            private NestedTree(int[][] array)
            {
                if (array.Length == 0 || array[0].Length == 0)
                {
                    return;
                }
                empty = false;
                arr = new int[array.Length, array[0].Length];
                arrl = array.Length;
                arrsi = array[0].Length - 1;
                for (int i = 0; i < array.Length; i++)
                {
                    for (int j = 0; j < array[0].Length; j++)
                    {
                        arr[i, j] = array[i][j];
                    }
                }

            }



            internal int GetSum(Dictionary<int, int> dictionary)
            {
                int sum = 0;
                int[,] vars = new int[dictionary.Count, 2];
                int idx = 0;
                foreach (var x in dictionary)
                {
                    vars[idx, 0] = x.Key;
                    vars[idx, 1] = x.Value;
                    idx++;
                }


                unsafe
                {
                    for (int i = 0; i < arrl; i++)
                    {
                        bool ok = true;

                        fixed (int* arrBase = &arr[i, 0])
                        {
                            for (int j = 0; j < dictionary.Count; j++)
                            {
                                if (arrBase[vars[j, 0]] != vars[j, 1])
                                {
                                    ok = false;
                                    break;
                                }
                            }
                        }
                        if (ok)
                        {
                            sum += arr[i, arrsi];
                        }
                    }
                }
                return sum;
            }

        }

        bool _dirty = true;
        [NonSerialized]
        private List<int[]> inferredData;
        private Node CPTMainNode;
        [NonSerialized]
        private Dictionary<int, int> inferenceVariables = new Dictionary<int, int>();
        [NonSerialized]
        private Dictionary<int, int> prevInferenceVarialbes = new Dictionary<int, int>();


        public int Total { get; set; }
        public int ObservedTotal { get; set; }
        public int InferredObservedTotal { get; protected set; }
        public int InferredTotal { get; private set; }
        private bool observationsExist;

        Dictionary<NPROB, int> columnIndexes = new Dictionary<NPROB, int>();



        public static CPT CreateCPT(Node n, Dictionary<Node, NPROB> data, IEnumerable<Trace> traces = null)
        {

            CPT cpt = new CPT();
            cpt.CPTMainNode = n;

            traces = traces ?? (from i in n.parentNodes from j in i.NodeObjects select j.trace).Union(n.NodeObjects.Select(x => x.trace)).Select(x => x).Distinct();



            Dictionary<Node, NPROB> nodes = new Dictionary<Node, NPROB>() { };
            n.parentNodes.ForEach(x => nodes.Add(x, data[x]));
            nodes.Add(n, data[n]);

            int colC = 0;
            foreach (NPROB x in nodes.Values)
            {
                cpt.columnIndexes.Add(x, colC);
                colC += x.columnsCount + 2;
            }


            Utility.NestedList nl = new Utility.NestedList(colC);

            foreach (Trace trace in traces)
            {
                List<int> result = new List<int>();
                foreach (var prob in nodes)
                {
                    List<int> value = prob.Value.GetValue(trace.name);
                    Node toTest = prob.Key;
                    for (int i = 1; i < trace.events.Count; i++)
                    {
                        if (trace.events[i].fullName.Contains(n.name) && trace.events[i - 1].fullName.Contains(toTest.name))
                        {
                            value[value.Count - 2] = 1;
                            break;
                        }
                    }
                    result.AddRange(value);
                }
                nl.IncrementValue(result);
            }

            int[][] arr = nl.ToArray();
            var startNode = n.parentNodes.Where(x => x.name == "start_event").FirstOrDefault();
            if (startNode != null)
            {
                int idx = cpt.columnIndexes[data[startNode]];
                for (int i = 0; i < arr.Length; i++)
                {
                    arr[i][idx] = 1;
                }
            }
            cpt.CPTable = arr;

            return cpt;
        }

        public string GetColumnRow()
        {
            string[] x = new string[aggregatedCPT[0].Length];
            foreach (var p in columnIndexes)
            {
                int startIdx = p.Value;
                var columns = p.Key.GetColumns();
                foreach (var c in columns)
                {
                    x[startIdx + c.Value] = c.Key;
                }
                x[startIdx + p.Key.columnsCount] = "previous";
                x[startIdx + p.Key.columnsCount + 1] = "occured";
            }

            StringBuilder sb = new StringBuilder();
            foreach (var s in x)
            {
                sb.Append(s + ";");
            }

            return sb.ToString();
        }

        public string GetObservedValues()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var p in columnIndexes)
            {
                int startIdx = p.Value;
                var columns = p.Key.GetColumns();
                StringBuilder s = new StringBuilder();
                foreach (var c in columns)
                {
                    int colIdx = startIdx + c.Value;
                    if (inferenceVariables.ContainsKey(colIdx))
                    {
                        s.Append(c.Key+":"+p.Key.invertedValues[c.Value][inferenceVariables[colIdx]]+"#^");
                       // s.Append(p.Key.invertedValues[c.Value][inferenceVariables[colIdx]] + ";");
                    }
                    else { s.Append("#^"); }
                }
                if (inferenceVariables.ContainsKey(startIdx + p.Key.columnsCount))
                {
                    s.Append("previous" + ":" + inferenceVariables[startIdx + p.Key.columnsCount] + "#^");
                    //s.Append(inferenceVariables[startIdx + p.Key.columnsCount] + ";");
                }
                else { s.Append("#^"); }
                if (inferenceVariables.ContainsKey(startIdx + p.Key.columnsCount+1))
                {
                    s.Append("occured" + ":" + inferenceVariables[startIdx + p.Key.columnsCount+1] + "#^");
                    //s.Append(inferenceVariables[startIdx + p.Key.columnsCount+1] + ";");
                }
                else { s.Append("#^"); }
                if (s.Length > 0)
                {
                    //sb.Append(p.Key.Name + "#%#");
                    sb.Append(s.ToString());
                    //sb.Append("#$");
                }
                //x[startIdx + p.Key.columnsCount] = "previous";
                //x[startIdx + p.Key.columnsCount + 1] = "occured";
            }
            return sb.ToString();
        }

        public string GetColumn(int idx)
        {
            int max = 0;
            foreach (var p in columnIndexes)
            {
                int startIdx = p.Value;
                var columns = p.Key.GetColumns();
                StringBuilder s = new StringBuilder();
                foreach (var c in columns)
                {
                    int colIdx = startIdx + c.Value;
                    if (colIdx== idx)
                    {
                        return p.Key.Name+"_"+c.Key;
                        // s.Append(p.Key.invertedValues[c.Value][inferenceVariables[colIdx]] + ";");
                    }
                }

                if (idx==startIdx + p.Key.columnsCount)
                {
                    return p.Key.Name + "_previous";
                    //s.Append(inferenceVariables[startIdx + p.Key.columnsCount] + ";");
                }

                if (idx == startIdx + p.Key.columnsCount + 1)
                {
                    return p.Key.Name + "_occured";
                } 
                               
            }
            return "count";
        }
        
        public IEnumerable<string> GetRow(int idx = 0, int count = Int32.MinValue)
        {

            int lastIdx = aggregatedCPT[0].Length;
            int wIdx = lastIdx - 2;
            int dim = aggregatedCPT.GetLength(0);
            List<int[]> result = new List<int[]>();


            for (int i = 0; i < dim; i++)
            {
                {
                    result.Add(aggregatedCPT[i]);
                    StringBuilder sb = new StringBuilder();
                    for (int k = 0; k < lastIdx; k++)
                    {


                        sb.Append(GetColumnValue(k, aggregatedCPT[i][k]) /*aggregatedCPT[i][k]*/ + ";");
                    }
                    yield return sb.ToString();
                }
            }
        }

        private string GetColumnValue(int colVal, int val)
        {
            string[] x = new string[aggregatedCPT[0].Length];
            foreach (var p in columnIndexes)
            {
                int startIdx = p.Value;
                var columns = p.Key.GetColumns();
                foreach (var c in columns)
                {
                    if (startIdx + c.Value == colVal)
                    {
                        return val >= 0 ? p.Key.invertedValues[c.Value][val] : val.ToString();
                    }
                }
            }
            return val.ToString();
        }

        public void ObserveVariable(string node, string variable, string value)
        {
            NPROB prob = (from i in columnIndexes where i.Key.Name == node select i.Key).FirstOrDefault();
            int innerIdx = default(int);
            int valueIdx = default(int);
            if (variable == "occured")
            {
                innerIdx = prob.columnsCount + 1;
                if (value == null)
                {
                    value = "1";
                }
                valueIdx = Int32.Parse(value);
            }
            else if (variable == "No data")
            {
                innerIdx = prob.columnsCount + 1;
                valueIdx = Int32.Parse(value);
            }
            else if (variable == "previous")
            {
                innerIdx = prob.columnsCount;
                valueIdx = Int32.Parse(value);
            }
            else
            {
                try
                {
                    innerIdx = prob.GetColumnIndex(variable);
                }
                catch (Exception exc)
                {
                    Console.WriteLine("Error. Cannot find variable {0}. ", variable, exc.ToString());
                    return;
                }
                valueIdx = -5;
                prob.values[innerIdx].TryGetValue(value, out valueIdx);
                if (innerIdx == prob.columnsCount)
                {
                    throw new Exception("cannot overwrite previous when it is not the chosen variable");
                }
                if (valueIdx < 0)
                {
                    return;
                }
            }
            int currIdx = columnIndexes[prob];

            if (valueIdx == 0 && innerIdx == prob.columnsCount)
            {
                throw new Exception("Why?");
            }
            int val = 0;
            if (inferenceVariables.TryGetValue(currIdx + innerIdx, out val))
            {
                inferenceVariables[currIdx + innerIdx] = valueIdx;
            }
            else
            {
                inferenceVariables.Add(currIdx + innerIdx, valueIdx);
            }

            _dirty = true;
            observationsExist = true;

        }
        

        Dictionary<string, double> ps = new Dictionary<string, double>();

        public double InferWhatIf(bool returnTotal = false)
        {
            return InferWhatIf(null, null, null, returnTotal,false);
        }
        public double InferWhatIfBasedOnTotal(string node, string variable, string value, bool returnTotal=false)
        {
            return InferWhatIf(node, variable, value, returnTotal, true);
        }
        public double InferWhatIfBasedOnOccured(string node, string variable, string value, bool returnTotal=false)
        {
            return InferWhatIf(node, variable, value, returnTotal, false);
        }

        private double InferWhatIf(string node, string variable, string value, bool returnTotal, bool basedOnTotal)
        {
            Dictionary<int, int> _infVar = new Dictionary<int, int>();
            string f = "";
            string conc = "";
            lock (this)
            {
                if (aggregatedCPT.Length == 0)
                {
                    return 1;
                }
                conc = node + variable + value;
                foreach (var iv in inferenceVariables)
                {
                    _infVar.Add(iv.Key, iv.Value);
                }
            }

            NPROB prob = (from i in columnIndexes where i.Key.Name == this.CPTMainNode.name select i.Key).FirstOrDefault();
            int innerIdx = prob.columnsCount;
            if (_infVar.ContainsKey(columnIndexes[prob] + innerIdx))
            {
                _infVar.Remove(columnIndexes[prob] + innerIdx);
            }

            var ivs = (from i in _infVar orderby i.Key select i);
            foreach (var iv in ivs)
            {
                conc += iv.Key.ToString() + iv.Value.ToString();
            }


            double r = double.NaN;

            #region transform arguments to dictionary
            if (!string.IsNullOrEmpty(node) && !string.IsNullOrEmpty(variable) && !string.IsNullOrEmpty(value))
            {
                prob = (from i in columnIndexes where i.Key.Name == node select i.Key).FirstOrDefault();
                bool add = true;
                if (prob != null)
                {
                    innerIdx = default(int);
                    int valueIdx = default(int);
                    if (variable == "occured")
                    {
                        innerIdx = prob.columnsCount + 1;
                        valueIdx = Int32.Parse(value);
                        if (valueIdx <= 0)
                        {
                            add = false;
                            var start = columnIndexes[prob];
                            var toDelete = new List<int>();
                            foreach (var x in _infVar)
                            {
                                if (x.Key > start) { toDelete.Add(x.Key); }
                            }
                            toDelete.ForEach(x => _infVar.Remove(x));
                        }
                    }
                    else if (variable == "No data")
                    {
                        innerIdx = prob.columnsCount + 1;
                        valueIdx = Int32.Parse(value);
                        if (valueIdx >= 0)
                        {
                            add = false;
                        }
                    }
                    else if (variable == "previous")
                    {
                        innerIdx = prob.columnsCount;
                        valueIdx = Int32.Parse(value);
                    }
                    else
                    {
                        innerIdx = prob.GetColumnIndex(variable);
                        valueIdx = prob.values[innerIdx][value];
                    }

                    if (add)
                    {
                        int currIdx = columnIndexes[prob];
                        if (valueIdx == 0 && innerIdx == prob.columnsCount - 1)
                        {
                            throw new Exception("Why?");
                        }
                        if (_infVar.ContainsKey(currIdx + innerIdx))
                        {

                            if (valueIdx < -1)
                            {
                                _infVar.Remove(currIdx + innerIdx);
                            }
                            else
                            {
                                _infVar[currIdx + innerIdx] = valueIdx;
                            }
                        }
                        else
                        {
                            if (valueIdx >= -1)
                            {
                                _infVar.Add(currIdx + innerIdx, valueIdx);
                            }
                        }
                    }
                }

            }
            else
            {
                //throw new ArgumentException("no arguments provided for inference");
            }
            #endregion

            lock (this)
            {
                f = returnTotal.ToString();
                _infVar.ToList().ForEach(x => { f += x.Key + x.Value; });
                if (ps.TryGetValue(f, out r))
                {
                    return r;
                }
            }

            lock (this)
            {
                double res = GetConditionedProbability2(returnTotal, _infVar, basedOnTotal);

                lock (ps)
                {
                    if (ps.TryGetValue(f, out r))
                    {

                        if (!double.Equals(r, res))
                            throw new ArgumentException("ajaja");
                        return r;
                    }
                    else
                    {
                        ps.Add(f, res);
                    }
                }
                return res;


            }
        }
        private double GetConditionedProbability2(bool returnTotal, Dictionary<int, int> _infVar, bool basedOnTotal)
        {
            int colC = aggregatedCPT[0].Length;

            int totalObservedConditionally = 0;
            int totalConditionally = 0;


            List<int> suitables = new List<int>();

            totalConditionally = basedOnTotal ? this.Total : tree.GetSum(_infVar);
            var x = new Dictionary<int, int>();
            foreach (var i in _infVar)
            {
                x.Add(i.Key, i.Value);
            }
            if (!x.ContainsKey(aggregatedCPT[0].Length - 2))
            {
                x.Add(aggregatedCPT[0].Length - 2, 1);
                totalObservedConditionally = tree.GetSum(x);
            }
            else
            {
                totalObservedConditionally = totalConditionally;
            }

            var pEH = ((double)totalObservedConditionally / totalConditionally);
            if (pEH > 1)
            {
                Console.WriteLine("oi");
            }

            return returnTotal ? totalObservedConditionally : pEH;
        }


        public void ClearObservations()
        {
            if (inferenceVariables.Count == 0)
            {
                return;
            }
            inferenceVariables = new Dictionary<int, int>();
            _dirty = true;
            observationsExist = false;
        }

        public string GetObservations()
        {
            string res = "";
            foreach (var x in inferenceVariables)
            {
                string field = "";
                string value = "";
                var probVal = (from i in columnIndexes where i.Value + i.Key.columnsCount >= x.Key && i.Value <= x.Key select i).FirstOrDefault();
                if (x.Key - probVal.Value == probVal.Key.columnsCount)
                {
                    field = "occured";
                    value = x.Value.ToString();
                }
                else
                {
                    field = probVal.Key.invertedColumns[x.Key - probVal.Value];
                    value = probVal.Key.invertedValues[x.Key - probVal.Value][x.Value];
                }

                res += field + "=" + value + ";";
            }
            return res;
        }

        public CPTGeneratedData GenerateEventData()
        {
            _Infer();
            
            
            int[] data = null;
            int occurrenceCount = 0;
            IEnumerable<int[]> usedData = null;
            int count = 0;
            int total = 0;
            if (InferredTotal == 0)
            {
                usedData = aggregatedCPT.Select(x => x);
                count = aggregatedCPT.Length;
                total = ObservedTotal;
            }
            else
            { 
                usedData = inferredData;
                count = inferredData.Count;
                total = InferredObservedTotal;  
                  
            }
            data = usedData.First();
            int val = rnd.Next(0, total);
            int sum = 0;
            foreach(var d in usedData)
            {
                if (d[d.Length - 2] <= 0)
                {
                    continue;
                }
                if (val > sum)
                {
                    occurrenceCount = d[d.Length - 1];
                    sum += occurrenceCount;
                }
                else
                {
                    data = d;
                    occurrenceCount = d[d.Length - 1];
                    break;
                }
            }
            

            var result = new Dictionary<string, string>();
            for(int i = 0; i < data.Length; i++)
            {
                var col = GetColumn(i);
                if (col.EndsWith("previous") || col.EndsWith("occured"))
                {
                    continue;
                }
                result.Add(GetColumn(i), GetColumnValue(i, data[i]));
            }


            return new CPTGeneratedData() { probability = occurrenceCount / (double)total, data = result };
        }

        private object pvlock = new object();

        public IEnumerable<int[]> Infer()
        {
            _Infer();

            foreach (int[] x in inferredData)
                yield return x;

        }
        private void _Infer()
        {
            if (!_dirty)
                return;

            lock (pvlock)
            {

            List<int[]> result = new List<int[]>();
            bool same = true;
            if (inferenceVariables.Count > 0)
            {
                if (inferenceVariables.Count == prevInferenceVarialbes.Count)
                {
                    foreach (var kv in inferenceVariables)
                    {
                        if (prevInferenceVarialbes.ContainsKey(kv.Key))
                        {
                            if (kv.Value != inferenceVariables[kv.Key])
                            {
                                same = false;
                                break;
                            }
                        }
                        else
                        {
                            same = false;
                            break;
                        }
                    }
                }
                else
                {
                    same = false;
                }

                if (same)
                {
                    _dirty = false;
                    return;
                }
                else
                {
                    prevInferenceVarialbes = new Dictionary<int, int>();
                    foreach (var kv in inferenceVariables)
                    {
                        prevInferenceVarialbes.Add(kv.Key, kv.Value);
                    }
                }
            }
            else
            {
                same = false;
                prevInferenceVarialbes = new Dictionary<int, int>();
            }


            int inferredObservedTotal = 0;
            int inferredTotal = 0;
            int dim = aggregatedCPT.Length;
            int colC = aggregatedCPT[0].Length;

            result = MakeInference(inferenceVariables);


            foreach (var y in result)
            {
                int val = y[colC - 1];
                inferredTotal += val;
                if (y[colC - 2] > 0)
                {
                    inferredObservedTotal += val;
                }
            }

            InferredTotal = inferredTotal;
            InferredObservedTotal = inferredObservedTotal;
            inferredData = result;
            _dirty = false;
            }
        }

        private List<int[]> MakeInference(Dictionary<int, int> _infVar)
        {
            List<int[]> result = null;
            if (_infVar.Count == 0)
            {
                result = new List<int[]>(); //aggregatedCPT.AsParallel().ToList();
            }
            else
            {
                result = aggregatedCPT.AsParallel().Where(y =>
                {
                    bool suitable = true;
                    foreach (var val in _infVar)
                    {
                        if (y[val.Key] != val.Value)
                        {
                            suitable = false;
                            break;
                        }
                    }
                    return suitable;

                }).ToList();
            }
            return result;
        }

        class InferenceResult
        {
            public int res;
        }
        public double ProbOfOccurence()
        {
            _Infer();

            double result = 0;

            if (inferenceVariables.Count == 0)
                return (double)ObservedTotal / (double)Total;

            result = (double)InferredObservedTotal / (double)InferredTotal;

            return result;
        }

        public double NonCausalProbOfOccurence()
        {
            return (double)ObservedTotal / (double)Total;
        }

        public bool Occured()
        {
            int lngt = aggregatedCPT[0].Length - 2;
            int lngtVal = default(int);
            if (inferenceVariables.TryGetValue(lngt, out lngtVal))
            {
                return true;
            }
            return false;
        }

    }
}
