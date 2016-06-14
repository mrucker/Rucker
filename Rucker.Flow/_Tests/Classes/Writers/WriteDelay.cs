using System.Threading;
using Rucker.Data;

namespace Rucker.Flow.Tests
{
    public class WriteDelay: IWrite<IRows>
    {
        private readonly int _delay;

        public WriteDelay(int delay)
        {
            _delay = delay;
        }

        public void Write(IRows data)
        {
            Thread.Sleep(_delay);
        }

        public void Dispose()
        {
            
        }
    }
}