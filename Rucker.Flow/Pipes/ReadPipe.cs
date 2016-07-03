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
        private class MostlyThreadSafePagedEnumerable : IEnumerable<T>
        {
            private readonly IRead<T> _reader;
            private readonly int _pageSize;

            public MostlyThreadSafePagedEnumerable(IRead<T> reader, int pageSize)
            {
                _reader = reader;
                _pageSize = pageSize;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return new MostlyThreadSafePagedEnumerator(_reader, _pageSize);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        /// <summary>
        /// The default Enumerator created by iterator blocks is blocking. That is, only one thread can read from it at a time. There are lots of good reasons for that (see https://codeblog.jonskeet.uk/2009/10/23/iterating-atomically/)
        /// Because we know exactly how our pipe will be called and used we can cheat a little bit to get around these shortcomings. This implementation is in no way safe for generalized use thus why I've made them private classes.
        /// </summary>
        [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
        private class MostlyThreadSafePagedEnumerator : IEnumerator<T>
        {
            [ThreadStatic]
            private static int LocalIndex;
            private static readonly ConcurrentDictionary<IRead<T>, int> GlobalIndex = new ConcurrentDictionary<IRead<T>, int>();
            private static readonly ConcurrentDictionary<IRead<T>, int> GlobalCount = new ConcurrentDictionary<IRead<T>, int>();
            private static readonly ConcurrentDictionary<IRead<T>, int> GlobalSize  = new ConcurrentDictionary<IRead<T>, int>();

            private readonly IRead<T> _reader;

            public MostlyThreadSafePagedEnumerator(IRead<T> reader, int size)
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

            public T Current => CurrentPage();

            object IEnumerator.Current => Current;

            private T CurrentPage()
            {
                return _reader.Read(LocalIndex * GlobalSize[_reader], GlobalSize[_reader]);
            }
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
            var readEnumerable = new MostlyThreadSafePagedEnumerable(reader, pageSize);

            return () => readEnumerable;
        }
        #endregion
    }
}