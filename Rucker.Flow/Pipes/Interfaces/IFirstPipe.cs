using System.Collections.Generic;

namespace Rucker.Flow
{
    public interface IFirstPipe<out P>: IPipe
    {
        IEnumerable<P> Produces { get; }

        void Stop();
    }

    public static class IFirstPipeExtensions
    {
        public static IFirstPipe<P2> Then<P1, C1, P2>(this IFirstPipe<P1> first, IMidPipe<C1, P2> mid) where P1 : class, C1
        {
            return new ConcatFirstMidPipe<P1, C1, P2>(first, mid);
        }

        public static IDonePipe Then<P1, C1>(this IFirstPipe<P1> first, ILastPipe<C1> last) where P1 : class, C1
        {
            return new ConcatFirstLastPipe<P1, C1>(first, last);
        }

        public static IFirstPipe<P> Thread<P>(this IFirstPipe<P> first, int maxDegreeOfParallelism) where P: class
        {            
            return new ConcatFirstMidPipe<P,P,P>(first, new ThreadedMidPipe<P>(maxDegreeOfParallelism));
        }

        public static IFirstPipe<P> Async<P>(this IFirstPipe<P> first) where P : class
        {
            return new ConcatFirstMidPipe<P, P, P>(first, new AsyncMidPipe<P>());
        }

        
    }
}