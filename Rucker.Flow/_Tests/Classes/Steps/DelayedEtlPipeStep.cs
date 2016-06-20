using Rucker.Data;

namespace Rucker.Flow.Tests
{
    public class DelayedEtlPipeStep : EtlCodeStep<IRows, IRows>
    {
        public DelayedEtlPipeStep(int delayTime)
        {
            Reader = new ReadDelay(delayTime / 2);
            Writer = new WriteDelay(delayTime / 2);
        }
    }
}