using System.Linq;
using Rucker.Core;

namespace Rucker.Flow.Tests
{
    class ReadMemory: IRead<IRows>
    {
        public IRows Memory { get; set; }

        public ReadMemory(params IRow[] objects)
        {
            Memory = objects.ToRows();
        }

        public int Size()
        {
            return Memory.Count();
        }

        public IRows Read(int skip, int take)
        {
            return Memory.Skip(skip).Take(take).ToRows();
        }

        public void Dispose()
        {
            
        }
    }
}
