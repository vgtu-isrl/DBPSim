using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BBNGs;
using BBNGs.Graph;
using BBNGs.TraceLog;

namespace BBNGs
{

    [Serializable]
    public class NPROB
    {
        public int columnsCount { get { return columns.Count; } }

        private Dictionary<string, int> columns { get; set; }
        public Dictionary<int, string> invertedColumns { get; private set; }
        public Dictionary<int, Dictionary<string, int>> values { get; private set; }
        public Dictionary<int, Dictionary<int, string>> invertedValues { get; private set; }
        public Dictionary<string, List<int>> data { get; private set; }
        public Dictionary<string, string> dataCS { get; private set; }

        public string Name { get { return node.name; } }
        protected Node node;

        public NPROB(Node node)
        {
            this.node = node;
            columns = new Dictionary<string, int>();
            invertedColumns = new Dictionary<int, string>();
            values = new Dictionary<int, Dictionary<string, int>>();
            invertedValues = new Dictionary<int, Dictionary<int, string>>();
            data = new Dictionary<string, List<int>>();
            dataCS = new Dictionary<string, string>();

            node.NodeObjects.ForEach(ne =>
            {
                AddEvData(ne);
            });
        }

        private string GetValue(Event e)
        {
            string trace = e.trace.name;
            if (e.fullName.Contains("endcomplete"))
                return "end";
            List<int> ed = null;
            if (!data.TryGetValue(e.trace.name, out ed))
            {
                ed = new List<int>(columns.Count);
                while (ed.Count < columns.Count)
                    ed.Add(-1);
            }
            StringBuilder sb = new StringBuilder();
            ed.ForEach(x => sb.Append("_" + x));

            return sb.ToString(1, sb.Length - 1);
        }

        private void AddEvData(Event e)
        {
            string trace = e.trace.name;
            foreach (var ev in e.EventVals)
            {
                AddValue(trace, ev.Key, ev.Value.value);
            }
        }

        private int ResolveColumn(string col)
        {
            int idx = default(int);

            if (!columns.TryGetValue(col, out idx))
            {
                idx = columns.Count;
                columns.Add(col, idx);
                invertedColumns.Add(idx, col);
                values.Add(idx, new Dictionary<string, int>());
                invertedValues.Add(idx, new Dictionary<int, string>());
                return idx;
            }
            return idx;
        }

        private int ResolveColumnValue(int col, string val)
        {
            int idx = default(int);
            Dictionary<string, int> colVals = values[col];
            if (!colVals.TryGetValue(val, out idx))
            {
                idx = colVals.Count;
                colVals.Add(val, idx);
                invertedValues[col].Add(idx, val);
                return idx;
            }
            return idx;
        }

        private List<int> AddDataValue(string traceid, int col, int val)
        {
            List<int> traceData = null;

            int cCnt = columns.Count;
            if (!data.TryGetValue(traceid, out traceData))
            {
                traceData = new List<int>(cCnt);
                data.Add(traceid, traceData);
            }

            while (traceData.Count < cCnt)
            {
                traceData.Add(-1);
            }
            traceData[col] = val;
            return traceData;
        }

        private void AddValue(string traceid, string col, string val)
        {
            int cid = ResolveColumn(col);
            int vid = ResolveColumnValue(cid, val);

            AddDataValue(traceid, cid, vid);
        }


        public List<int> GetValue(string trace)
        {
            List<int> result = null;
            if (data.TryGetValue(trace, out result))
            {
                result = result.ToList();
                result.Add(-1);
                result.Add(1);
            }
            else
            {
                result = new List<int>(columns.Count);
                while (result.Count < columns.Count)
                {
                    result.Add(-1);
                }
                result.Add(-1);
                result.Add(-1);
            }
            return result;
        }

        public IEnumerable<KeyValuePair<string, int>> GetColumns()
        {
            foreach (var col in columns)
            {
                yield return col;
            }

        }

        public int GetColumnIndex(string value)
        {
            return columns[value];
        }







    }

}
