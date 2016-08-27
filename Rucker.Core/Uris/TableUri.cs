using System;

namespace Rucker.Core
{
    public class TableUri: BaseUri
    {
        #region Properties
        public DatabaseUri DatabaseUri { get; }
        public string SchemaName { get; }
        public string TableName { get; }
        #endregion

        #region Constructors
        public TableUri(DatabaseUri database, string table, string schema = "dbo"): this(database.ServerName, database.DatabaseName, table, schema)
        { }

        public TableUri(string server, string database, string table, string schema = "dbo"): this($"table://{server}/{database}/{schema}/{table}")
        { }

        public TableUri(string uriString): base(uriString)
        {
            if (Scheme.ToLower() != "table")
            {
                throw new UriFormatException("A table URI must have a 'table://' scheme.");
            }

            if (Parts.Length < 3 || 4 < Parts.Length)
            {
                throw new UriFormatException("The proper format for a table URI is table://server/database/[schema]/table");
            }

            DatabaseUri   = new DatabaseUri($"database://{Parts[0]}/{Parts[1]}");
            SchemaName = Parts.Length == 4 ? Parts[2] : "dbo";
            TableName  = Parts.Length == 4 ? Parts[3] : Parts[2];
        }
        #endregion

        #region Public Methods
        public string ToFullyQualifiedTableName()
        {
            return $"{DatabaseUri.DatabaseName}.{SchemaName}.{TableName}";
        }
        #endregion
    }
}