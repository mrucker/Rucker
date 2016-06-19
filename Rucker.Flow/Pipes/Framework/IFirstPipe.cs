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
            return new FirstMidCoupling<P1, C1, P2>(first, mid);
        }

        public static IDonePipe Then<P1, C1>(this IFirstPipe<P1> first, ILastPipe<C1> last) where P1: class, C1
        {
            return new FirstLastCoupling<P1, C1>(first, last);
        }
    }
}