using System;

namespace Rucker.Data
{
    public class UriWriter: IWrite<IRows>
    {
        private readonly IWrite<IRows> _writer;

        public UriWriter(string uriString, int? timeout = null): this(uriString.ToUri(), timeout)
        {

        }

        public UriWriter(BaseUri destUri, int? timeout = null)
        {
            _writer  = GetWriter(destUri, timeout);
        }

        private IWrite<IRows> GetWriter(BaseUri destUri, int? timeout)
        {
            if (destUri.Scheme.ToLower() == "table")
            {
                throw new NotImplementedException();
                //var tableUri = (TableUri)destUri;
                //return new WriteConvert<IRows, IBulk>(new WriteBulk(tableUri.DatabaseUri.ToSqlQuerier(), timeout), rows => new Bulk(tableUri.TableName, rows));
            }

            throw new UriFormatException("The scheme for the GenericInsert SourceUri isn't supported.");
        }

        public void Write(IRows data)
        {
            _writer.Write(data);
        }

        public void Dispose()
        {
            _writer.Dispose();
        }
    }
}