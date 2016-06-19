using System.Collections.Generic;

namespace Rucker.Flow
{
    public interface IMidPipe<in C, out P>: IPipe
    {
        IEnumerable<P> Produces { get; }
        IEnumerable<C> Consumes { set; }
    }

    public static class IMidPipeExtensions
    {
        public static IMidPipe<C1, P2> Then<C1, P1, C2, P2>(this IMidPipe<C1,P1> mid1, IMidPipe<C2, P2> mid2) where P1 : class, C2
        {
            return new ConcatMidMidPipe<C1, P1, C2, P2>(mid1, mid2);
        }

        public static IMidPipe<P, P> Thread<P>(this IMidPipe<P, P> mid, int maxDegreeOfParallelism) where P : class
        {
            return new ConcatMidMidPipe<P, P, P, P>(mid, new ThreadMidPipe<P>(maxDegreeOfParallelism));
        }

        public static ILastPipe<C1> Then<C1, P1, C2>(this IMidPipe<C1, P1> mid, ILastPipe<C2> last) where P1 : class, C2
        {
            return new ConcatMidLastPipe<C1, P1, C2>(mid, last);
        }
    }
}

