using System;

namespace Rucker.Data
{
    public class WriteConvert<TSource, TDest>: IWrite<TSource>
    {
        #region Fields
        private readonly IWrite<TDest> _writer;
        private readonly Func<TSource, TDest> _converter;
        #endregion

        #region Constructors
        public WriteConvert(IWrite<TDest> writer, Func<TSource, TDest> converter)
        {
            _writer = writer;
            _converter = converter;
        }
        #endregion

        #region IWrite<TSource> Implementation
        public void Write(TSource source)
        {
            _writer.Write(_converter(source));
        }

        public void Dispose()
        {
            _writer.Dispose();
        }
        #endregion
    }
}