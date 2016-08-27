using System;
using Rucker.Core;

namespace Rucker.Flow.Tests
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