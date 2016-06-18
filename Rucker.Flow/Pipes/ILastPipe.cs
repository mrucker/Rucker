using System.Collections.Generic;

namespace Rucker.Flow
{
    public interface ILastPipe<in C>: IPipe
    {
        IEnumerable<C> Consumes { set; }

        void Start();
    }
}