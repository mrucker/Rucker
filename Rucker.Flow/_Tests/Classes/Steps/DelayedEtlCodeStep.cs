using Rucker.Data;

namespace Rucker.Flow.Tests
{
    public class DelayedEtlCodeStep : EtlCodeStep<IRows, IRows>
    {
        public DelayedEtlCodeStep(int delayTime)
        {
            
        }
    }
}