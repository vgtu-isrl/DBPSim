using DBPSim.RuleEngine.DataSource;
using DBPSim.RuleEngine.Memory;
using System;
using System.Collections.Generic;
using System.Xml;

namespace DBPSim.RuleEngine
{
    public class RuleCollection : List<RuleBase>
    {

        public RuleCollection() : base()
        {
        }


        public static RuleCollection Load(DataSourceProvider dataSource)
        {
            if (dataSource == null)
            {
                throw new ArgumentNullException("dataSource");
            }
            RuleCollection allRules = dataSource.Load();
            if (dataSource is IDisposable)
            {
                ((IDisposable)dataSource).Dispose();
            }
            return allRules;
        }

    }
}
