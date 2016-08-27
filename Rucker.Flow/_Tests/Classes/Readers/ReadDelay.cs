using System.Linq;
using System.Threading;
using Rucker.Core;

namespace Rucker.Flow.Tests
{
    public class ReadDelay: IRead<IRows>
    {
        private readonly int _delay;

        public ReadDelay(int delay)
        {
            _delay = delay;
        }

        public void Dispose()
        {
            
        }

        public int Size()
        {
            return 12000;
        }

        public IRows Read(int skip, int take)
        {
            Thread.Sleep(_delay);
            
            return Enumerable.Empty<IRow>().ToRows();
        }
    }
}