using Rucker.Data;

namespace Rucker.Flow.Tests
{
    public class FailedEtlStep : EtlCodeStep<IRows, IRows>
    {
        public FailedEtlStep()
        {

        }
    }
}