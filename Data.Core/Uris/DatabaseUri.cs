using System;
using System.Linq;
using System.Configuration;
using System.Data.EntityClient;

namespace Data.Core
{
    public class DatabaseUri: BaseUri
    {
        public string ServerName { get; private set; }
        public string DatabaseName { get; private set; }

        public static DatabaseUri FromNameOrConnectionString(string nameOrConnectionString)
        {
            var parts = ConnectionString(nameOrConnectionString).ToLower().Split(';').Where(p => p.IsNotNullOrEmpty()).ToDictionary(p => p.Split('=')[0].Trim(), p => p.Split('=')[1].Trim());

            return new DatabaseUri(parts["server"], parts["database"]);
        }

        public DatabaseUri(string server, string database): this("database://" + server + "/" + database)
        { }

        public DatabaseUri(string uriString): base(uriString)
        {
            if(Scheme.ToLower() != "database")
            {
                throw new UriFormatException("A database URI must have a 'database://' scheme.");
            }

            if (Parts.Length != 2)
            {
                throw new UriFormatException("The proper format for a database URI is database://server/database");
            }
            
            ServerName   = Parts[0];
            DatabaseName = Parts[1];
        }

        #region Private Methods
        private static string ConnectionString(string nameOrConnectionString)
        {
            if (!nameOrConnectionString.StartsWith("name="))
            {
                return nameOrConnectionString;
            }

            var name = nameOrConnectionString.Replace("name=", "");
            var connectionString = ConfigurationManager.ConnectionStrings[name].ConnectionString;

            if (connectionString.Contains("metadata="))
            {
                connectionString = new EntityConnectionStringBuilder(connectionString).ProviderConnectionString;
            }

            return connectionString;
        }
        #endregion
    }
}