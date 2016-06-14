namespace Rucker.Data
{
    public class DbQuerier: IQuerier
    {
        #region Fields     
        private readonly string _nameOrConnectionString;
        #endregion
        
        #region Properties
        public string Server => QuerierUtility.Server(_nameOrConnectionString);
        public string Database => QuerierUtility.Database(_nameOrConnectionString);
        #endregion

        #region Constructors
        public DbQuerier(DatabaseUri databaseUri): this(databaseUri.ServerName, databaseUri.DatabaseName)
        { }

        public DbQuerier(string server, string database): this($"Server={server}; Database={database}; Integrated Security=True;")
        { }

        public DbQuerier(string nameOrConnectionString)
        {
            _nameOrConnectionString = nameOrConnectionString;
        }
        #endregion

        #region Public Methods
        public DbQuerierConnection BeginConnection()
        {
            return new DbQuerierConnection(_nameOrConnectionString);
        }

        IQuerierConnection IQuerier.BeginConnection()
        {
            return BeginConnection();
        }
        #endregion
    }
}