using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBPSim.RuleEngine.Memory
{
    public static class WatchPoints
    {

        private static ExpandoObject _watchPoints = null;


        static WatchPoints()
        {
            Init();
        }


        public static void Init()
        {
            _watchPoints = new ExpandoObject();
        }


        public static void Init(ExpandoObject obj)
        {
            _watchPoints = obj;
        }


        public static object Instance
        {
            get
            {
                return _watchPoints;
            }
        }

    }
}
