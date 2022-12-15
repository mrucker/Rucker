using System;
using Data.Core;

namespace Data.Flow.Tests
{
    public class WriteFailed: IWrite<IRows>
    {
        public void Write(IRows data)
        {
            throw new Exception();
        }

        public void Dispose()
        { }
    }
}