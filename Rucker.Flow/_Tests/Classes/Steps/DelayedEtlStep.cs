using Rucker.Data;

namespace Rucker.Flow.Tests
{
    public class DelayedEtlStep : EtlStep<IRows, IRows>
    {
        public DelayedEtlStep(int delayTime)
        {
            Reader = new ReadDelay(delayTime/2);
            Writer = new WriteDelay(delayTime/2);
        }
    }

}