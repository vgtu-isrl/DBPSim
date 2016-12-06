using System;
using System.Data;
using System.Data.SqlClient;

namespace DBPSim.RuleEngine.DataSource
{
    public class DatabaseDataSourceProvider : DataSourceProvider, IDisposable
    {

        SqlConnection _connection = null;

        public DatabaseDataSourceProvider(SqlConnection sqlConnection)
        {
            throw new NotImplementedException();
        }


        public override RuleCollection Load()
        {
            if (this._connection.State != ConnectionState.Open)
            {
                this._connection.Open();
            }
            throw new NotImplementedException();
        }


        public override void Save(RuleCollection ruleCollection)
        {
            if (this._connection.State != ConnectionState.Open)
            {
                this._connection.Open();
            }
            throw new NotImplementedException();
        }


        public void Dispose()
        {
            if (this._connection != null)
            {
                this._connection.Dispose();
            }
        }

    }
}
