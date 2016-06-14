using System;

namespace Rucker.Data
{
    /// <summary>
    /// Takes a Reader and a Convert Lambda to convert a reader into another type  
    /// </summary>
    /// <typeparam name="TSource">The Type of the Original Reader</typeparam>
    /// <typeparam name="TDest">The Type of the New Reader</typeparam>
    public class ReadConvert<TSource, TDest>: IRead<TDest>
    {
        #region Fields
        private readonly IRead<TSource> _reader;
        private readonly Func<TSource, TDest> _toDest;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of ReadConvert
        /// </summary>
        /// <param name="reader">Reader we want to convert</param>
        /// <param name="toDest">Lambda to convert Reader to TDest</param>
        public ReadConvert(IRead<TSource> reader, Func<TSource, TDest> toDest)
        {
            _reader = reader;
            _toDest = toDest;
        }
        #endregion

        #region IRead<TDest> Implementation
        public void Dispose()
        {
            _reader.Dispose();
        }

        public int Size()
        {
            return _reader.Size();
        }

        public TDest Read(int skip, int take)
        {
            return _toDest(_reader.Read(skip, take));
        }
        #endregion
    }
}