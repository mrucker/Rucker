namespace Rucker.Flow
{
    public interface IMidPipe<in C, out P>: IFirstPipe<P>, ILastPipe<C>
    {
        
    }

    public static class IMidPipeExtensions
    {
        public static IMidPipe<C1, P2> Then<C1, P1, C2, P2>(this IMidPipe<C1,P1> mid1, IMidPipe<C2, P2> mid2) where P1 : class, C2
        {
            return new MidPipesPipe<C1, P1, C2, P2>(mid1, mid2);
        }

        public static ILastPipe<C1> Then<C1, P1, C2>(this IMidPipe<C1, P1> mid, ILastPipe<C2> last) where P1 : class, C2
        {
            return new LastPipesPipe<C1, P1, C2>(mid, last);
        }
    }
}

