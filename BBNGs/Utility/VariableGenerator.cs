using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBNGs.VariableGenerators
{
    class VariableGenerator
    {
        public Random r { get; private set; }
        public List<string> values { get; private set; }
        public List<double> distr { get; private set; }

        public VariableGenerator(List<string> values, Random r)
        {
            this.values = values;
            this.r = r;
            distr = new List<double>();
            distr.Add(0);
            for (int i = 0; i < this.values.Count - 1; i++)
                distr.Add(r.NextDouble());
            distr.Add(1);
            distr = distr.OrderBy(x => x).ToList();
        }

        public string Next()
        {
            double val = r.NextDouble();

            for (int i = 0; i < distr.Count; i++)
            {
                if (val > distr[i] && val < distr[i + 1])
                    return values[i];
            }
            return null;
        }

        public static void ExplainGenerator(VariableGenerator gen)
        {
            Dictionary<string, int> pasiskirstymas = new Dictionary<string, int>();
            //Generator gen = new Generator(data, new Random(1235));

            foreach (string s in gen.values)
                pasiskirstymas.Add(s, 0);

            for (int i = 0; i <= 10000; i++)
            {
                Console.WriteLine("\r" + i);
                string val = gen.Next();
                pasiskirstymas[val]++;
            }
            Console.WriteLine();

            for (int i = 0; i < pasiskirstymas.Count; i++)
            {
                string el = pasiskirstymas.Keys.ElementAt(i);
                Console.WriteLine(el + "=>" + (int)((gen.distr[i + 1] - gen.distr[i]) * 10000) + "=>" + pasiskirstymas[el]);
            }
        }
    }
}
