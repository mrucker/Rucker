using System;
using System.IO;
using Rucker.Dispose;

namespace Rucker.Data
{
    public class ReadUri: Disposable, IRead<IRows>
    {
        #region Fields
        private readonly IRead<IRows> _reader;
        #endregion

        #region Constructors
        public ReadUri(string uriString) : this(uriString.ToUri())
        {

        }

        public ReadUri(BaseUri sourceUri)
        {
            _reader = GetReader(sourceUri);
        }
        #endregion

        #region Public Methods
        public int Size()
        {
            DisposeCheck();

            return _reader.Size();
        }

        public IRows Read(int skip, int take)
        {
            DisposeCheck();

            return _reader.Read(skip, take);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _reader.Dispose();

            base.Dispose(disposing);
        }
        #endregion

        #region Private Methods
        private static IRead<IRows> GetReader(BaseUri sourceUri)
        {
            if (sourceUri.Scheme.ToLower() == "table")
            {
                return GetTableUriReader(sourceUri);
            }

            if (sourceUri.Scheme.ToLower() == "file")
            {
                return GetFileUriReader(sourceUri);
            }

            throw new UriFormatException($"The {sourceUri.Scheme} uri scheme isn't recognized by the ReadUri.");
        }

        private static IRead<IRows> GetFileUriReader(BaseUri uri)
        {
            var fileUri   = uri as FileUri ?? new FileUri(uri.ToString());
            var extension = Path.GetExtension(fileUri.FilePath);

            if (extension == ".csv")
            {
                return new ReadCsvFile(fileUri.FilePath);
            }

            throw new UriFormatException("The file extension (" + extension + ") isn't recognized by ReadUri.");
        }

        private static IRead<IRows> GetTableUriReader(BaseUri uri)
        {
            var tableUri   = uri as TableUri ?? new TableUri(uri.ToString());
            var tableQuery = $"SELECT * FROM {tableUri.SchemaName}.{tableUri.TableName}";

            return new ReadSqlServerPre2012(tableUri.DatabaseUri.ToSqlQuerier(), tableQuery);
        }
        #endregion

    }
}