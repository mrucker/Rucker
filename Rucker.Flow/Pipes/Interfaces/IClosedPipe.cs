namespace Rucker.Flow
{
    public interface IClosedPipe: IPipe
    {
        void Start();
        void Stop();
    }

    public static class IDonePipeExtensions
    {
        public static IClosedPipe Thread(this IClosedPipe closed, int maxDegreeOfParallelism)
        {
            return new ThreadedClosedPipe(closed, maxDegreeOfParallelism);
        }

        public static IClosedPipe Async(this IClosedPipe closed)
        {
            return new AsyncClosedPipe(closed);
        }
    }
}