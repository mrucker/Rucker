using System.Collections.Generic;

namespace Rucker.Flow
{
    public interface IFirstPipe<out P>: IPipe
    {
        IEnumerable<P> Produces { get; }
    }

    public static class IFirstPipeExtensions
    {
        public static IFirstPipe<P2> Then<P1, C1, P2>(this IFirstPipe<P1> first, IMidPipe<C1, P2> mid) where P1 : class, C1
        {
            return new FirstPipesPipe<P1, C1, P2>(first, mid);
        }

        public static IPipe Then<P1, C1>(this IFirstPipe<P1> first, ILastPipe<C1> last) where P1: class, C1
        {
            return new DonePipesPipe<P1, C1>(first, last);
        }
    }
}