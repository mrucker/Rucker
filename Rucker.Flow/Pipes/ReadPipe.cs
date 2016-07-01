using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Rucker.Data;

namespace Rucker.Flow
{
    public class ReadPipe<T>: LambdaFirstPipe<T>
    {
        #region Classes
        private class GlobalReadEnumerable : IEnumerable<T>
        {
            private readonly IRead<T> _reader;
            private readonly int _pageSize;

            public GlobalReadEnumerable(IRead<T> reader, int pageSize)
            {
                _reader = reader;
                _pageSize = pageSize;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return new GlobalReadEnumerator(_reader, _pageSize);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
        private class GlobalReadEnumerator : IEnumerator<T>
        {
            [ThreadStatic]
            private static int LocalIndex;
            private static readonly ConcurrentDictionary<IRead<T>, int> GlobalIndex = new ConcurrentDictionary<IRead<T>, int>();
            private static readonly ConcurrentDictionary<IRead<T>, int> GlobalCount = new ConcurrentDictionary<IRead<T>, int>();
            private static readonly ConcurrentDictionary<IRead<T>, int> GlobalSize  = new ConcurrentDictionary<IRead<T>, int>();

            private readonly IRead<T> _reader;

            public GlobalReadEnumerator(IRead<T> reader, int size)
            {
                _reader = reader;
                
                GlobalSize.TryAdd(reader, size);
                GlobalCount.TryAdd(reader, reader.PageCount(size));
                GlobalIndex.TryAdd(reader, 0);

                if (size != GlobalSize[reader])
                {
                    throw new ArgumentException("Multiple enumerators can iterate over the same reader with different page sizes!!!!");
                }
            }

            public void Dispose()
            {
                throw new NotImplementedException("We dispose of the reader straight in the ReadPipe so there is no need to call this");
            }

            public bool MoveNext()
            {
                lock (_reader)
                {
                    LocalIndex = GlobalIndex[_reader];
                    GlobalIndex[_reader] = LocalIndex + 1;
                }

                return LocalIndex < GlobalCount[_reader];
            }

            public void Reset()
            {
                GlobalIndex[_reader] = -1;
            }

            public T Current => _reader.Read(LocalIndex * GlobalSize[_reader], GlobalSize[_reader]);

            object IEnumerator.Current => Current;
        }
        #endregion

        #region Fields
        private readonly IRead<T> _reader;
        #endregion

        #region Constructor
        public ReadPipe(IRead<T> reader, int pageSize) : base(Read(reader, pageSize))
        {
            _reader = reader;
        }

        #endregion

        #region Disposable Pattern
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _reader.Dispose();
            }

            base.Dispose(disposing);
        }
        #endregion

        #region Private Methods
        private static Func<IEnumerable<T>> Read(IRead<T> reader, int pageSize)
        {
            var readEnumerable = new GlobalReadEnumerable(reader, pageSize);

            return () => readEnumerable;
        }
        #endregion
    }
}