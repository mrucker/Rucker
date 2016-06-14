using System;
using Rucker.Data;
using Rucker.Flow;

namespace Rucker.Flow.Tests.Classes
{
    public class FailedEtlJob : EtlJob<IRows, IRows>
    {
        public FailedEtlJob()
        {
            Reader = new ReadMemory();
            Writer = new WriteFailed();
        }
    }
}