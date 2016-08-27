using System;
using System.Data;
using Rucker.Dispose;

namespace Rucker.Sql
{
    ///<summary>
    /// A minimal implementation of IDataReader. 
    /// The only Methods and Properties that are implemented are those that are needed for SqlBulkCopy
    /// </summary>
    /// <remarks>
    /// Implementation taken from http://www.developerfusion.com/article/122498/using-sqlbulkcopy-for-high-performance-inserts/
    /// </remarks>
    public abstract class DataReader: Disposable, IDataReader
    {
        #region Abstract
        public abstract int Count { get; }
        #endregion

        #region IDataReader Abstract
        public abstract bool IsClosed { get; }
        public abstract int FieldCount { get; }
        public abstract bool Read();
        public abstract void Close();
        public abstract int GetOrdinal(string name);
        public abstract object GetValue(int i);
        #endregion

        #region IDataReader NotImplemented
        bool IDataReader.NextResult()
        {
            throw new NotImplementedException();
        }

        int IDataReader.Depth
        {
            get { throw new NotImplementedException(); }
        }

        DataTable IDataReader.GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        int IDataReader.RecordsAffected
        {
            get { throw new NotImplementedException(); }
        }

        bool IDataRecord.IsDBNull(int i)
        {
            throw new NotImplementedException();
        }

        object IDataRecord.this[int i]
        {
            get { throw new NotImplementedException(); }
        }

        object IDataRecord.this[string name]
        {
            get { throw new NotImplementedException(); }
        }

        int IDataRecord.GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        bool IDataRecord.GetBoolean(int i)
        {
            throw new NotImplementedException();
        }

        byte IDataRecord.GetByte(int i)
        {
            throw new NotImplementedException();
        }

        long IDataRecord.GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        char IDataRecord.GetChar(int i)
        {
            throw new NotImplementedException();
        }

        long IDataRecord.GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        Guid IDataRecord.GetGuid(int i)
        {
            throw new NotImplementedException();
        }

        short IDataRecord.GetInt16(int i)
        {
            throw new NotImplementedException();
        }

        int IDataRecord.GetInt32(int i)
        {
            throw new NotImplementedException();
        }

        long IDataRecord.GetInt64(int i)
        {
            throw new NotImplementedException();
        }

        float IDataRecord.GetFloat(int i)
        {
            throw new NotImplementedException();
        }

        double IDataRecord.GetDouble(int i)
        {
            throw new NotImplementedException();
        }

        string IDataRecord.GetString(int i)
        {
            throw new NotImplementedException();
        }

        decimal IDataRecord.GetDecimal(int i)
        {
            throw new NotImplementedException();
        }

        DateTime IDataRecord.GetDateTime(int i)
        {
            throw new NotImplementedException();
        }

        IDataReader IDataRecord.GetData(int i)
        {
            throw new NotImplementedException();
        }

        string IDataRecord.GetName(int i)
        {
            throw new NotImplementedException();
        }

        string IDataRecord.GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        Type IDataRecord.GetFieldType(int i)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}