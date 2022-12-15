using System;
using System.Configuration;
using System.Data.EntityClient;

namespace Data.Core
{
    public interface IQuerier
    {
        string Server { get; }
        string Database { get; }

        IQuerierConnection BeginConnection();
    }

    public static class QuerierUtility
    {
        public static string Database(string nameOrConnectionString)
        {
            return new System.Data.SqlClient.SqlConnectionStringBuilder(ConnectionString(nameOrConnectionString)).InitialCatalog;
        }

        public static string Server(string nameOrConnectionString)
        {
            return new System.Data.SqlClient.SqlConnectionStringBuilder(ConnectionString(nameOrConnectionString)).DataSource;
        }

        public static string ConnectionString(string nameOrConnectionString)
        {
            if (nameOrConnectionString.StartsWith("name="))
            {
                var name = nameOrConnectionString.Replace("name=", "");

                if (ConfigurationManager.ConnectionStrings[name] == null)
                {
                    throw new ArgumentException($"Couldn't find connection string {name}", nameof(nameOrConnectionString));
                }

                var connectionString = ConfigurationManager.ConnectionStrings[name].ConnectionString;

                if (connectionString.Contains("metadata="))
                {
                    connectionString = new EntityConnectionStringBuilder(connectionString).ProviderConnectionString;
                }

                return connectionString;
            }

            return nameOrConnectionString;
        }
    }
}