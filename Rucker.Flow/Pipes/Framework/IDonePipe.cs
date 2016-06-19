namespace Rucker.Flow
{
    public interface IDonePipe: IPipe
    {
        void Start();
        void Stop();
    }

    public static class IDonePipeExtensions
    {
        public static IDonePipe Thread(this IDonePipe done, int maxDegreeOfParallelism)
        {
            return new ThreadedDonePipe(done, maxDegreeOfParallelism);
        }

        public static IDonePipe Async(this IDonePipe done)
        {
            return new AsyncDonePipe(done);
        }
    }
}