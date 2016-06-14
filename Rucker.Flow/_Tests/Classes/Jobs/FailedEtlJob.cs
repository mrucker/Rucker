using Rucker.Data;

namespace Rucker.Flow.Tests
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