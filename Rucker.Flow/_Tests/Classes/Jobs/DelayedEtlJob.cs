using Rucker.Data;
using Rucker.Flow;

namespace Rucker.Flow.Tests.Classes
{
    public class DelayedEtlJob : EtlJob<IRows, IRows>
    {
        public DelayedEtlJob(int delayTime)
        {
            Reader = new ReadDelay(delayTime/2);
            Writer = new WriteDelay(delayTime/2);
        }
    }

}