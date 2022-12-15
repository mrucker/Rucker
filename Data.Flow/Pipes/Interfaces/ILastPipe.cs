using System.Collections.Generic;

namespace Data.Flow
{
    public interface ILastPipe<in C>: IPipe
    {
        IEnumerable<C> Consumes { set; }

        void Start();
    }
}