using Rucker.Data;

namespace Rucker.Flow.Tests
{
    public class FailedEtlStep : EtlStep<IRows, IRows>
    {
        public FailedEtlStep()
        {
            Reader = new ReadMemory();
            Writer = new WriteFailed();
        }
    }
}