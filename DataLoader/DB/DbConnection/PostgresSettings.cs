using System.Collections.Generic;
using System.Linq;
using LinqToDB.Configuration;
using Npgsql;

namespace DataLoader.DB.DbConnection
{
    public class PostgresSettings : ILinqToDBSettings
    {
        public IEnumerable<IDataProviderSettings> DataProviders => Enumerable.Empty<IDataProviderSettings>();
        public string DefaultConfiguration => "PostgreSQL.9.5";
        public string DefaultDataProvider => "PostgreSQL.9.5";
        public IEnumerable<IConnectionStringSettings> ConnectionStrings {
            get
            {
                yield return
                    new ConnectionStringSettings
                    {
                        Name = "fb_database",
                        ProviderName = "PostgreSQL.9.5",
                        ConnectionString = PostgresConnectionString
                    };
            }
        }

        public string PostgresConnectionString = new NpgsqlConnectionStringBuilder
        {
            Username = "postgres",
            Password = "postgres",
            Database = "fb_database",
            Host = "127.0.0.1",
            Port = 5432
        }.ToString();
    }
}