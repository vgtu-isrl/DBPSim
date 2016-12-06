using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBNGs
{
    [Serializable]
    public class SquareMatrix
    {
        public Dictionary<string, int> columns { get; private set; }
        public Dictionary<string, int> rows { get; private set; }

        public List<List<double>> data = new List<List<double>>();
        public List<List<int>> valueCount = new List<List<int>>();

        private bool dirty = true;
        private double _deviation = 0;
        public double Deviation
        {
            get
            {
                if (dirty)
                    CalculateDeviation();
                return _deviation;
            }
        }
        private double _variance = 0;
        public double Variance
        {
            get
            {
                if (dirty)
                    CalculateDeviation();
                return _variance;
            }
        }
        private double _alphaLevel = 0;
        public double AlphaLevel
        {
            get
            {
                if (dirty)
                    CalculateDeviation();
                return _alphaLevel;
            }
        }
        private double _squaredAlphaLevel = 0;
        public double SquaredAlphaLevel
        {
            get
            {
                if (dirty)
                    CalculateDeviation();
                return _squaredAlphaLevel;
            }
        }
        
        private void CalculateDeviation()
        {
            _deviation = Math.Sqrt(GetVariance());
            _alphaLevel = _deviation * 0.02;
            _squaredAlphaLevel = Math.Pow(_alphaLevel, 2);
            dirty = false;
        }
         
        private double GetVariance()
        {
            double sum = 0;
            int nonemptycount = 0;
            for (int i = 0; i < data.Count; i++)
            {
                for (int j = 0; j < data.Count; j++)
                {
                    double value = data[i][j];
                    sum += value;
                    if (value > 0)
                        nonemptycount++;
                }
            }

            double mean = sum/Math.Pow(nonemptycount, 2);

            sum = 0;

            for (int i = 0; i < data.Count; i++)
            {
                for (int j = 0; j < data.Count; j++)
                {
                    sum += Math.Pow((data[i][j] - mean), 2);
                }
            }
            int n = nonemptycount > 3 ? nonemptycount - 1 : nonemptycount;
            _variance = sum / n;

            return _variance;
        }

        public void TransferCountsAsData()
        {
            for(int i=0;i<data.Count;i++)
            {
                for(int j=0;j<data[i].Count;j++)
                {
                    data[i][j] = valueCount[i][j];
                }
            }
        }

        public SquareMatrix()
        {
            columns = new Dictionary<string, int>();
            rows = new Dictionary<string, int>();
        }

        public void AddColumn(string columnName)
        {
            columns.Add(columnName, columns.Count);
            rows.Add(columnName, rows.Count);
            while (data.Count < rows.Count)
                data.Add(new List<double>());
            while (valueCount.Count < rows.Count)
                valueCount.Add(new List<int>());

            foreach (var row in data)
            {
                while (row.Count < columns.Count)
                    row.Add(0);
            }
            foreach (var row in valueCount)
            {
                while (row.Count < columns.Count)
                    row.Add(0);
            }
            dirty = true;
        }

        public void TryAddColumn(string column)
        {
            if (!columns.Keys.Contains(column))
                AddColumn(column);
            dirty = true;
        }

        public void AddValue(string column, string row, double value)
        {
            int cIdx = columns[column];
            int rIdx = rows[row];

            data[cIdx][rIdx] = value;
            valueCount[cIdx][rIdx]++;
            dirty = true;
        }

        public void AddValue(int column, int row, double value)
        {

            data[column][row] = value;
            valueCount[column][row]++;
            dirty = true;
        }

        public SquareMatrix CopyEmpty()
        {
            SquareMatrix matrix = new SquareMatrix();
            foreach(var col in columns)
            {
                matrix.TryAddColumn(col.Key);
            }
            return matrix;
        }

        public List<string> GetNonEmptyColumnRows(string columnName, double cutoff)
        {
            List<string>result = new List<string>();
            List<double> column = data[columns[columnName]];

            double cutoffVal = Math.Abs(cutoff);

            var rowNs = cutoff >= 0 ? rows.Where(x => column[x.Value] > cutoffVal) : rows.Where(x => column[x.Value] < cutoffVal);

            foreach (var rowN in rowNs)
                result.Add(rowN.Key);

            return result;
        }

        public List<string> GetNonEmptyRowColumns(string rowName, double cutoff)
        {
            List<string> result = new List<string>();
            int rowIdx = rows[rowName];

            double cutoffVal = Math.Abs(cutoff);

            var colNs = cutoff >= 0 ? columns.Where(x => data[x.Value][rowIdx] > cutoffVal) : columns.Where(x => data[x.Value][rowIdx] < cutoffVal);

            foreach(var colN in colNs)
            {
                    result.Add(colN.Key);
            }

            return result;
        }

        public List<double> this[string column]
        {
            get
            {
                return data[columns[column]];
            }
        }

        public List<double> this[int column]
        {
            get
            {
                return data[column];
            }
        }

        public double GetValue(string columnName, string rowName)
        {
            return this.data[columns[columnName]][rows[rowName]];
        }

    }
}
