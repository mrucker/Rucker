using System;
using System.Threading.Tasks;

namespace Data.Flow
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

        public static void AsyncStart(this IClosedPipe closed)
        {
            if (closed.Status != PipeStatus.Working)
            {
                Task.Run((Action)closed.Start);
            }
        }
    }
}