
namespace DBPSim.RuleEngine.DataSource
{
    public abstract class DataSourceProvider
    {

        public abstract RuleCollection Load();
        public abstract void Save(RuleCollection ruleCollection);

    }
}
