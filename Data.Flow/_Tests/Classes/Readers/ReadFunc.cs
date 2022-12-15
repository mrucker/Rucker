using System;
using System.Collections.Generic;
using Data.Core;

namespace Data.Flow.Tests
{
    public class ReadFunc:IRead<string>
    {
        private readonly Func<IEnumerable<string>> _enumerableFactory;
        private IEnumerator<string> _itemEnumerator;

        public ReadFunc(Func<IEnumerable<string>> read)
        {
            _enumerableFactory = read;
        }

        public void Dispose()
        { }

        public int Size()
        {
            return 3;
        }

        public string Read(int skip, int take)
        {
            _itemEnumerator = _itemEnumerator ?? _enumerableFactory().GetEnumerator();

            if (!_itemEnumerator.MoveNext())
            {
                _itemEnumerator = null;
                return Read(skip, take);                
            }

            return _itemEnumerator.Current;
        }
    }
}