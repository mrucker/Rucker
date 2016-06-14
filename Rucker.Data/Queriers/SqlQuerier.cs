namespace Rucker.Data
{
    public class SqlQuerier: IQuerier
    {
        #region Fields
        private readonly string _nameOrConnectionString;
        #endregion

        #region  Properties
        public string Server => QuerierUtility.Server(_nameOrConnectionString);
        public string Database => QuerierUtility.Database(_nameOrConnectionString);
        #endregion

        #region Constructors
        public SqlQuerier(DatabaseUri databaseUri): this(databaseUri.ServerName, databaseUri.DatabaseName)
        { }

        public SqlQuerier(string server, string database): this($"Server={server}; Database={database}; Integrated Security=True;")
        { }

        public SqlQuerier(string nameOrConnectionString)
        {
            _nameOrConnectionString = nameOrConnectionString;
        }
        #endregion

        #region Public Methods
        public SqlQuerierConnection BeginConnection()
        {
            return new SqlQuerierConnection(_nameOrConnectionString);
        }

        IQuerierConnection IQuerier.BeginConnection()
        {
            return BeginConnection();
        }
        #endregion
    }
}