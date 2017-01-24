using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace BBNGs
{
    public class Utility
    {
        public static void OutputMatrix(Dictionary<string, SquareMatrix> mtrs, string fileName)
        {
            using (StreamWriter sw = new StreamWriter(File.Open(Environment.CurrentDirectory + @"\" + fileName, FileMode.Create, FileAccess.Write),Encoding.UTF8))
            {
                foreach (var m in mtrs)
                {

                    SquareMatrix mtr = m.Value;
                    sw.WriteLine(m.Key);
                    sw.Write(",");
                    foreach (string val in mtr.columns.Keys)
                    {
                        sw.Write(val + ",");
                    }
                    sw.Write("\r\n");

                    foreach (var row in mtr.rows.Keys)
                    {
                        sw.Write(row + ",");
                        foreach (var val in mtr.data[mtr.rows[row]])
                        {
                            sw.Write(val + ",");
                        }
                        sw.Write("\r\n");
                    }
                    sw.WriteLine();
                }
            }
        }
        static List<T> ReturnList<T>() where T : class
        {
            return new List<T>();
        }

        public class DependenceFinder
        {
            public bool significant = false;
            public List<CausalDependencyValue> significances = new List<CausalDependencyValue>();

            public DependenceFinder(dynamic ar1, dynamic ar2, IEnumerable<dynamic> val1, IEnumerable<dynamic> val2)
            {

                var values1 = val1.ToList();
                var values2 = val2.ToList();
                Dictionary<dynamic, int> valueCounts = new Dictionary<dynamic, int>();
                Dictionary<bool, Dictionary<dynamic, Dictionary<dynamic, int>>> dataValues1 = CreateValueDictionary(values1, values2, valueCounts);
                int count = 0;
              

                for (int i = 0; i < ar1.Count; i++)
                {
                    if (ar2.Count <= i)
                        break;

                    count++;
                    dynamic sk = ar1[i];
                    dynamic sk2 = ar2[i];

                    dataValues1[true][sk][sk2]++;

                    valueCounts[sk]++;

                    for (int idx = 0; idx < values1.Count; idx++)
                    {
                        dynamic v = values1[idx];
                        if (v != sk)
                        {
                            dataValues1[false][v][sk2]++;
                        }
                    }
                }

                Parallel.ForEach(values1, new Action<dynamic>((dynamic x) => {
                    foreach (dynamic y in values2)
                    {

                        int tx = dataValues1[true][x][y];
                        int fx = dataValues1[false][x][y];
                        int ts = valueCounts[x];
                        int fs = count - ts;
                        double px = (double)tx / (double)ts;
                        double pnx = (double)fx / (double)fs;
                        CausalDependencyValue s = new CausalDependencyValue() { x = x.ToString(), y = y.ToString(), px = px, pnx = pnx, ts = ts, fs = fs };
                        if (ts + fs > 100 && ts > 30)
                        {
                            if (Math.Abs(px - pnx) >= 0.05)
                            {
                                significant = true;
                                s.significant = true;
                            }
                        }
                        lock(significances)
                        {
                            significances.Add(s);
                        }
                    }
                }));

                //foreach (dynamic x in values1)
                //{
                //    foreach (dynamic y in values2)
                //    {
                //        int tx = dataValues1[true][x][y];
                //        int fx = dataValues1[false][x][y];
                //        int ts = valueCounts[x];
                //        int fs = count - ts;
                //        double px = (double)tx / (double)ts;
                //        double pnx = (double)fx / (double)fs;
                //        CausalDependencyValue s = new CausalDependencyValue() { x = x.ToString(), y = y.ToString(), px = px, pnx = pnx,ts=ts,fs=fs };
                //        if (ts + fs > 100 && ts > 30)
                //        {
                //            if (Math.Abs(px - pnx) >= 0.05)
                //            {
                //                significant = true;
                //                s.significant = true;
                //            }
                //        }

                //        significances.Add(s);
                //    }
                //}

            }
            
            private static Dictionary<bool, Dictionary<dynamic, Dictionary<dynamic, int>>> CreateValueDictionary(dynamic values1, dynamic values2, dynamic valueCounts)
            {
                Dictionary<bool, Dictionary<dynamic, Dictionary<dynamic, int>>> dataValues = new Dictionary<bool, Dictionary<dynamic, Dictionary<dynamic, int>>>();

                dataValues.Add(true, new Dictionary<dynamic, Dictionary<dynamic, int>>());
                dataValues.Add(false, new Dictionary<dynamic, Dictionary<dynamic, int>>());

                foreach (dynamic x in values1)
                {
                    valueCounts.Add(x, 0);
                    dataValues[true].Add(x, new Dictionary<dynamic, int>());
                    dataValues[false].Add(x, new Dictionary<dynamic, int>());
                    foreach (dynamic y in values2)
                    {
                        dataValues[true][x].Add(y, 0);
                        dataValues[false][x].Add(y, 0);
                    }
                }
                return dataValues;
            }

            public class CausalDependencyValue
            {
                public string x;
                public string y;
                public int ts;
                public int fs;
                public double px;
                public double pnx;
                public bool significant = false;
            }

        }

       

        public class NestedList
        {
            dynamic dict = new Dictionary<int, int>();
            protected int _depth = 0;

            public NestedList(int depth)
            {
                int currDepth = 1;
                _depth = 0;

                Type t = dict.GetType();
                while (currDepth < depth)
                {
                    Type nt = typeof(Dictionary<,>).MakeGenericType(typeof(int),t);
                    t = nt;
                    dict = Activator.CreateInstance(nt);
                    currDepth++;
                }
                _depth = currDepth;
            }

            public void SetValue(List<int> path, int value)
            {
                int x = 0;
                string pathRep = string.Concat( path);
                if (!valueIdx.TryGetValue(pathRep, out x))
                {
                    int idx = valueIdx.Count;
                    valueIdx.Add(pathRep, valueIdx.Count);
                    values[idx]=value;
                }
                else
                {
                    values[x] = value;
                }
            }

            public void IncrementValue(List<int> path)
            {
                int x = 0;
                string pathRep = string.Concat( path);
                if (!valueIdx.TryGetValue(pathRep, out x))
                {
                    string p = string.Join("_", path);
                    transformations.Add(pathRep, p);
                    int idx = valueIdx.Count;
                    valueIdx.Add(pathRep, idx);
                    values.Add(idx, 1);
                }
                else
                {
                    values[x] += 1;
                }
            }

            private dynamic CheckPath(List<int> path)
            {
                dynamic obj = dict;
                dynamic tmp = null;
                for(int i=0;i<path.Count;i++)
                {
                    if(obj.TryGetValue(path[i],out tmp))
                    {
                        obj = tmp;
                    }
                    else
                    {
                        Type t = obj.GetType().GenericTypeArguments[1];
                        obj.Add(path[i], Activator.CreateInstance(t));
                    }                    
                }
               
                return obj;
            }

            private string GetStringRepOfPath(List<int> path)
            {
                return string.Concat( path);
            }

            Dictionary<string, int> valueIdx = new Dictionary<string, int>();
            Dictionary<string, string> transformations = new Dictionary<string, string>();
            Dictionary<int, int> values = new Dictionary<int, int>();

            public int[][] ToArray()
            {
                int[][] result = new int[valueIdx.Count][];

                int rIdx = 0;
                foreach (var v in valueIdx)
                {
                    int cIdx = 0;
                    List<string> els = transformations[v.Key].Split(new char[] { '_' }).ToList();
                    result[rIdx] = new int[_depth+1];
                    els.ForEach(x =>
                        {
                            result[rIdx][cIdx] = Int32.Parse(x);
                            cIdx++;
                        });
                    result[rIdx][cIdx] = values[v.Value];
                    rIdx++;
                }

                return result;
            }

        }



    }

    public class IntegerValuePair
    {
        public int Key = 0;
        public int Value = 0;
    }

    public static class TransformationUtility
    {

        protected class Item
        {
            public string header;
            public string description;
            public List<string> content = new List<string>();
        }

        public static void TransformAnomaliesIntoHtml(string src, string dst)
        {
            List<string> cnt = new List<string>();
            using (StreamReader sr = new StreamReader(src))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (!string.IsNullOrEmpty(line))
                    {
                        cnt.Add(line);
                    }
                }
            }

            List<Item> items = new List<Item>();
            for (var i = 0; i < cnt.Count; i++)
            {
                var header = cnt[i];
                if (!header.StartsWith("============"))
                {
                    continue;
                }


                var csidx = i + 2;

                var csedx = i + 3;

                while (csedx < cnt.Count && !cnt[csedx].StartsWith("=============="))
                {
                    csedx++;
                }

                if (csidx > cnt.Count)
                {
                    continue;
                }

                var content = cnt.Skip(csidx).Take(csedx - csidx).ToList();

                var sequence = content[0];
                var anomalyDesc = content[1];
                var anomalyCtx = content.Skip(2).ToList();

                Item itm = new Item() { header = sequence, description = anomalyDesc, content = anomalyCtx };
                items.Add(itm);
            }

            using (StreamWriter sw = new StreamWriter(dst))
            {
                sw.WriteLine("<html><head>");

                sw.WriteLine("<link rel=\"stylesheet\" href=\"styles/tablestyle.css\"/>");

                sw.WriteLine("A List of anomalies");


                sw.WriteLine("<table>");

                var idx = 0;
                foreach (var anomaly in items)
                {
                    sw.WriteLine("<tr style='height:80px;'></tr>");
                    sw.WriteLine("<tr><td>{0}</td><td>", ++idx);

                    sw.WriteLine("Anomaly occured during the following event sequence:<br/>");
                    sw.WriteLine("<i>{0}</i><br/>", System.Web.HttpUtility.HtmlEncode(anomaly.header.Replace("Parents:", "")));
                    sw.WriteLine("<b>Description:</b><i>{0}</i><br/>", System.Web.HttpUtility.HtmlEncode(anomaly.description).Replace("Parents:", "").Replace("Probabilistic anomaly for n", "N").Replace(" Expected any of:", ""));

                    if (anomaly.content.Count > 0)
                    {
                        sw.WriteLine("Predecessor of the event status:<br/>");
                        sw.WriteLine("<table class=\"\"><tr><th>Node</th><th>Occured</th></tr>");

                        foreach (var c in anomaly.content)
                        {
                            var cc = c.Split(new string[] { " has occured=" }, StringSplitOptions.RemoveEmptyEntries);
                            sw.WriteLine("<tr><td>{0}</td><td>{1}</td></tr>", cc[0], cc.Length > 1 ? cc[1] : String.Empty);
                        }

                        sw.WriteLine("</table>");
                    }
                    sw.WriteLine("</td></tr>");
                }

                sw.WriteLine("</table>");

                sw.WriteLine("</head><body>");

            }
        }

        public static void TransformRulesIntoHtml(string ruleSrc, string ruleDest)
        {
            List<string> cnt = new List<string>();
            using (StreamReader sr = new StreamReader(ruleSrc))
            {
                while (!sr.EndOfStream)
                {
                    cnt.Add(sr.ReadLine());
                }
            }

            List<Item> items = new List<Item>();
            for (var i = 0; i < cnt.Count; i++)
            {
                var header = cnt[i];
                if (!header.StartsWith("//Sequence"))
                {
                    continue;
                }
                header = header.Remove(0, "//Sequence ".Length);

                var headerCnt = header.Split(new string[] { "=>" }, StringSplitOptions.None);

                var csidx = i + 2;

                var csedx = i + 3;

                while (csedx < cnt.Count && !cnt[csedx].StartsWith("//Sequence") && (int)cnt[csedx][0] > 48 && (int)cnt[csedx][0] < 57)
                {
                    csedx++;
                }
                var content = cnt.Skip(csidx + 1).Take(csedx - csidx - 1).ToList();

                Item itm = new Item();

                itm.header = headerCnt[0];
                itm.description = headerCnt[1];

                foreach (var c in content)
                {
                    itm.content.Add("With confidence " + c.Replace(";", " AND "));
                }
                i = csedx - 1;
                items.Add(itm);
            }

            using (StreamWriter sw = new StreamWriter(ruleDest))
            {
                sw.WriteLine("<html><head>");

                sw.WriteLine("<link rel=\"stylesheet\" href=\"styles/tablestyle.css\"/>");


                sw.WriteLine("</head><body>");

                var grouped = items.GroupBy(x => x.header);

                sw.WriteLine("<table class=\"responsetable\"><tr><th>Start Event</th><th>End Event</th><th>AssociativeRules</th></tr>");
                foreach (var g in grouped)
                {
                    foreach (var g2 in g)
                    {
                        if (g2.content.Count == 0)
                        {
                            continue;
                        }
                        var fst = System.Web.HttpUtility.HtmlEncode(g.Key);
                        var snd = System.Web.HttpUtility.HtmlEncode(g2.description);
                        if (fst.Length > 15)
                        {
                            fst = fst.Insert(fst.Length / 2, "<br/>");
                        }
                        if (snd.Length > 15)
                        {
                            snd = snd.Insert(snd.Length / 2, "<br/>");
                        }
                        sw.WriteLine("<tr><td>" + fst + "</td><td>" + snd + "</td><td>");

                        sw.WriteLine("<OL>");
                        foreach (var r in g2.content)
                        {

                            sw.WriteLine("<LI>" + System.Web.HttpUtility.HtmlEncode(r) + "</LI>");
                        }
                        sw.WriteLine("</OL>");
                        sw.WriteLine("</td></tr>");


                    }
                }
                sw.WriteLine("</table>");
                sw.WriteLine("</body></html>");
            }
        }

    }
}

namespace Apriori
{


    public class ItemsDictionary : KeyedCollection<List<string>, Item>
    {
        protected override List<string> GetKeyForItem(Item item)
        {
            return item.Elements;
        }

        new public Item this[List<string> key]
        {
            get {
                foreach(var x in this.Items)
                {
                    if(x.Elements.Count != key.Count)
                    {
                        continue;
                    }

                    var suits = true;
                    for(var i = 0; i < x.Elements.Count;i++)
                    {
                        if (!key.Contains(x.Elements[i]))
                        {
                            suits = false;
                            break;
                        }
                    }
                    if (suits)
                    {
                        return x;
                    }
                }
                return null;
            }
        }


        internal void ConcatItems(IList<Item> frequentItems)
        {
            foreach (var item in frequentItems)
            {
                this.Add(item);
            }
        }
    }

    public class Rule : IComparable<Rule>
    {
        #region Member Variables

        List<string> combination, remaining;
        double confidence;

        #endregion

        #region Constructor

        public Rule(List<string> combination, List<string> remaining, double confidence)
        {
            this.combination = combination;
            this.remaining = remaining;
            this.confidence = confidence;
        }

        #endregion

        #region Public Properties

        public List<string> X { get { return combination; } }

        public List<string> Y { get { return remaining; } }

        public double Confidence { get { return confidence; } }

        #endregion

        #region IComparable<clssRules> Members

        public int CompareTo(Rule other)
        {
            if(other.X.Count != X.Count)
            {
                return -1;
            }
            foreach(var x in X)
            {
                if (!other.X.Contains(x))
                {
                    return 1;
                }
            }
            return 0;
        }

        #endregion

        public override int GetHashCode()
        {
            ISorter sorter = new Sorter();
            var y = X.Select(x => x).ToList();
            y.AddRange(Y);
            string sortedXY = String.Join("", sorter.Sort(y));
            return sortedXY.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as Rule;
            if (other == null)
            {
                return false;
            }

            return this.GetHashCode() == other.GetHashCode();
        }
    }

    public class Item : IComparable<Item>
    {
        #region Public Properties

        public List<string> Elements { get; set; }
        public double Support { get; set; }

        #endregion

        #region IComparable

        public int CompareTo(Item other)
        {
            foreach(var x in other.Elements)
            {
                if (!this.Elements.Contains(x))
                {
                    return 0;
                }
            }
            return 1;
        }

        #endregion
    }

    public class Output
    {
        #region Public Properties

        public IList<Rule> StrongRules { get; set; }

        public IList<List<string>> MaximalItemSets { get; set; }

        public Dictionary<List<string>, Dictionary<List<string>, double>> ClosedItemSets { get; set; }

        public ItemsDictionary FrequentItems { get; set; }

        #endregion
    }



    public interface IApriori
    {
        Output ProcessTransaction(double minSupport, double minConfidence, List<List<string>> items, List<List<string>> transactions);
    }

    class Sorter : ISorter
    {
        List<string> ISorter.Sort(List<string> token)
        {
            return token.OrderBy(x => x).ToList();
        }
    }

    interface ISorter
    {
        List<string> Sort(List<string> token);
    }

    internal static class ContainerProvider
    {
        private static CompositionContainer container;

        public static CompositionContainer Container
        {
            get
            {
                if (container == null)
                {
                    List<AssemblyCatalog> catalogList = new List<AssemblyCatalog>();
                    catalogList.Add(new AssemblyCatalog(typeof(ISorter).Assembly));
                    container = new CompositionContainer(new AggregateCatalog(catalogList));
                }

                return container;
            }
        }
    }

    public class Apriori : IApriori
    {
        #region Member Variables

        readonly ISorter _sorter;

        #endregion

        #region Constructor

        public Apriori()
        {
            _sorter = new Sorter();
        }

        #endregion

        #region IApriori

        Output IApriori.ProcessTransaction(double minSupport, double minConfidence, List<List<string>> items, List<List<string>> transactions)
        {
            IList<Item> frequentItems = GetL1FrequentItems(minSupport, items, transactions);
            ItemsDictionary allFrequentItems = new ItemsDictionary();
            allFrequentItems.ConcatItems(frequentItems);
            IDictionary<List<string>, double> candidates = new Dictionary<List<string>, double>();
            double transactionsCount = transactions.Count();

            do
            {
                candidates = GenerateCandidates(frequentItems, transactions);
                frequentItems = GetFrequentItems(candidates, minSupport, transactionsCount);
                allFrequentItems.ConcatItems(frequentItems);
            }
            while (candidates.Count != 0);

            HashSet<Rule> rules = GenerateRules(allFrequentItems);
            IList<Rule> strongRules = GetStrongRules(minConfidence, rules, allFrequentItems);
            Dictionary<List<string>, Dictionary<List<string>, double>> closedItemSets = GetClosedItemSets(allFrequentItems);
            IList<List<string>> maximalItemSets = GetMaximalItemSets(closedItemSets);

            return new Output
            {
                StrongRules = strongRules,
                MaximalItemSets = maximalItemSets,
                ClosedItemSets = closedItemSets,
                FrequentItems = allFrequentItems
            };
        }

        #endregion

        #region Private Methods

        private List<Item> GetL1FrequentItems(double minSupport, List<List<string>> items, List<List<string>> transactions)
        {
            var frequentItemsL1 = new List<Item>();
            double transactionsCount = transactions.Count();

            foreach (var item in items)
            {
                double support = GetSupport(item, transactions);

                if (support / transactionsCount >= minSupport)
                {
                    frequentItemsL1.Add(new Item { Elements = item, Support = support });
                }
            }
            frequentItemsL1.Sort();
            return frequentItemsL1;
        }

        private double GetSupport(List<string> generatedCandidate, List<List<string>> transactionsList)
        {
            double support = 0;

            foreach (List<string> transaction in transactionsList)
            {
                if (CheckIsSubset(generatedCandidate, transaction))
                {
                    support++;
                }
            }

            return support;
        }

        private bool CheckIsSubset(List<string> child, List<string> parent)
        {
            if (!parent.Contains(child[0]))
            {
                return false;
            }
            var start = parent.IndexOf(child[0]);
            for(var i = 0; i < child.Count; i++)
            {
                if (start + i > parent.Count - 1)
                {
                    return false;
                }
                if (child[i] != parent[start + i])
                {
                    return false;
                }
            }           
            return true;
            
        }

        private Dictionary<List<string>, double> GenerateCandidates(IList<Item> frequentItems, List<List<string>> transactions)
        {
            Dictionary<List<string>, double> candidates = new Dictionary<List<string>, double>();

            for (int i = 0; i < frequentItems.Count - 1; i++)
            {
                List<string> firstItem = _sorter.Sort(frequentItems[i].Elements);

                for (int j = i + 1; j < frequentItems.Count; j++)
                {
                    List<string> secondItem = _sorter.Sort(frequentItems[j].Elements);
                    List<string> generatedCandidate = GenerateCandidate(firstItem, secondItem);

                    if (generatedCandidate.Count != 0)
                    {
                        double support = GetSupport(generatedCandidate, transactions);
                        candidates.Add(generatedCandidate, support);
                    }
                }
            }

            return candidates;
        }

        private List<string> GenerateCandidate(List<string> firstItem, List<string> secondItem)
        {
            int length = firstItem.Count;

            if (length == 1)
            {
                var y= firstItem.Select(x => x).ToList();
                y.AddRange(secondItem);
                return y;
            }
            else
            {

                
                List<string> firstSubString = firstItem.Take(length-1).ToList();
                List<string> secondSubString = secondItem.Take(length-1).ToList();
                var equals = true;
                for (var i = 0; i < firstSubString.Count; i++)
                {
                    if(firstSubString[i] != secondSubString[i])
                    {
                        equals = false;
                        break;
                    }
                }
                if (equals)
                {
                    var r = firstItem.Select(x => x).ToList();
                    r.Add(secondItem[length - 1]);
                    return r;
                }

                return new List<string>();
            }
        }

        private List<Item> GetFrequentItems(IDictionary<List<string>, double> candidates, double minSupport, double transactionsCount)
        {
            var frequentItems = new List<Item>();

            foreach (var item in candidates)
            {
                if (item.Value / transactionsCount >= minSupport)
                {
                    frequentItems.Add(new Item { Elements = item.Key, Support = item.Value });
                }
            }

            return frequentItems;
        }

        private Dictionary<List<string>, Dictionary<List<string>, double>> GetClosedItemSets(ItemsDictionary allFrequentItems)
        {
            var closedItemSets = new Dictionary<List<string>, Dictionary<List<string>, double>>();
            int i = 0;

            foreach (var item in allFrequentItems)
            {
                Dictionary<List<string>, double> parents = GetItemParents(item.Elements, ++i, allFrequentItems);

                if (CheckIsClosed(item.Elements, parents, allFrequentItems))
                {
                    closedItemSets.Add(item.Elements, parents);
                }
            }

            return closedItemSets;
        }

        private Dictionary<List<string>, double> GetItemParents(List<string> child, int index, ItemsDictionary allFrequentItems)
        {
            var parents = new Dictionary<List<string>, double>();

            for (int j = index; j < allFrequentItems.Count; j++)
            {
                List<string> parent = allFrequentItems[j].Elements;

                if (parent.Count == child.Count + 1)
                {
                    if (CheckIsSubset(child, parent))
                    {
                        parents.Add(parent, allFrequentItems[parent].Support); //TODO
                    }
                }
            }

            return parents;
        }

        private bool CheckIsClosed(List<string> child, Dictionary<List<string>, double> parents, ItemsDictionary allFrequentItems)
        {
            foreach (List<string> parent in parents.Keys)
            {
                if (allFrequentItems[child].Support == allFrequentItems[parent].Support)
                {
                    return false;
                }
            }

            return true;
        }

        private IList<List<string>> GetMaximalItemSets(Dictionary<List<string>, Dictionary<List<string>, double>> closedItemSets)
        {
            var maximalItemSets = new List<List<string>>();

            foreach (var item in closedItemSets)
            {
                Dictionary<List<string>, double> parents = item.Value;

                if (parents.Count == 0)
                {
                    maximalItemSets.Add(item.Key);
                }
            }

            return maximalItemSets;
        }

        private HashSet<Rule> GenerateRules(ItemsDictionary allFrequentItems)
        {
            var rulesList = new HashSet<Rule>();

            foreach (var item in allFrequentItems)
            {
                if (item.Elements.Count > 1)
                {
                    List<List<string>> subsetsList = GenerateSubsets(item.Elements);

                    foreach (var subset in subsetsList)
                    {
                        List<string> remaining = GetRemaining(subset, item.Elements);
                        Rule rule = new Rule(subset, remaining, 0);
                        

                        if (!rulesList.Contains(rule))
                        {
                            rulesList.Add(rule);
                        }
                    }
                }
            }

            return rulesList;
        }

        private List<List<string>> GenerateSubsets(List<string> item)
        {
            List<List<string>> allSubsets = new List<List<string>>();
            int subsetLength = item.Count / 2;

            for (int i = 1; i <= subsetLength; i++)
            {
                IList<List<string>> subsets = new List<List<string>>();
                List<string> tmp = new List<string>(item.Count);
                for(var j = 0; j <= item.Count; j++)
                {
                    tmp.Add(String.Empty);
                }
                GenerateSubsetsRecursive(item, i, tmp, subsets);
                allSubsets.AddRange(subsets);
            }

            return allSubsets;
        }

        private void GenerateSubsetsRecursive(List<string> item, int subsetLength, List<string> temp, IList<List<string>> subsets, int q = 0, int r = 0)
        {
            if (q == subsetLength)
            {
                List<string> sb = new List<string>();

                for (int i = 0; i < subsetLength; i++)
                {
                    sb.Add(temp[i]);
                }

                subsets.Add(sb);
            }

            else
            {
                for (int i = r; i < item.Count; i++)
                {
                    temp[q] = item[i];
                    GenerateSubsetsRecursive(item, subsetLength, temp, subsets, q + 1, i + 1);
                }
            }
        }

        private List<string> GetRemaining(List<string> child, List<string> parent)
        {
            parent = parent.Select(x => x).ToList(); // DANGER
            for (int i = 0; i < child.Count; i++)
            {
                parent.Remove(child[i]);
            }

            return parent;
        }

        private IList<Rule> GetStrongRules(double minConfidence, HashSet<Rule> rules, ItemsDictionary allFrequentItems)
        {
            var strongRules = new List<Rule>();

            foreach (Rule rule in rules)
            {
                var z = rule.X.Select(x => x).ToList();
                z.AddRange(rule.Y);
                List<string> xy = _sorter.Sort(z);
                AddStrongRule(rule, xy, strongRules, minConfidence, allFrequentItems);
            }

            strongRules.Sort();
            return strongRules;
        }

        private void AddStrongRule(Rule rule, List<string> XY, List<Rule> strongRules, double minConfidence, ItemsDictionary allFrequentItems)
        {
            double confidence = GetConfidence(rule.X, XY, allFrequentItems);

            if (confidence >= minConfidence)
            {
                Rule newRule = new Rule(rule.X, rule.Y, confidence);
                strongRules.Add(newRule);
            }

            confidence = GetConfidence(rule.Y, XY, allFrequentItems);

            if (confidence >= minConfidence)
            {
                Rule newRule = new Rule(rule.Y, rule.X, confidence);
                strongRules.Add(newRule);
            }
        }

        private double GetConfidence(List<string> X, List<string> XY, ItemsDictionary allFrequentItems)
        {
            var supX = allFrequentItems[X];
            var supXY = allFrequentItems[XY];
            double supportX = supX != null? supX.Support:0;
            double supportXY = supXY != null ? supXY.Support:0;
            return supportXY / supportX;
        }

        #endregion
    }
}
