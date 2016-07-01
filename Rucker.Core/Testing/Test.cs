using System;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Rucker.Testing
{
    public static class Test
    {
        #region Consts
        public const string ConnectionString = @"name=LocalDb";

        public const decimal MegaByte = 1048576M;
        #endregion

        #region Public Methods
        public static TimeSpan ExecutionTime(Action action)
        {
            var executionTime = new Stopwatch();

            executionTime.Start();
            action();
            executionTime.Stop();

            return executionTime.Elapsed;
        }

        public static TimeSpan ExecutionTime(int threadCount, int iterationCount, Action action)
        {
            var executionTime = new Stopwatch();

            Action<int> writeActionTime = i =>
            {
                var startTime = executionTime.Elapsed;
                action();
                var stopTime = executionTime.Elapsed;

                Console.WriteLine($"TokenTaken: {executionTime.Elapsed}");
            };

            executionTime.Start();
            if (threadCount == 1)
            {
                for (var i = 0; i < iterationCount; i++) writeActionTime(i);
            }
            else
            {
                Parallel.For(0, iterationCount, new ParallelOptions { MaxDegreeOfParallelism = threadCount }, writeActionTime);
            }
            executionTime.Stop();

            return executionTime.Elapsed;
        }

        public static decimal ExecutionBits(Func<object> function)
        {
            var beforeBits = GC.GetTotalMemory(true);
            var r = function();
            var afterBits = GC.GetTotalMemory(true);

            return afterBits - beforeBits;
        }

        public static string ClassNameOnly(object obj)
        {
            return obj.ToString() == obj.GetType().FullName ? obj.GetType().Name : obj.ToString();
        }

        public static void WriteLine(string message)
        {
            if (message == null) return;

            Console.WriteLine($"{DateTime.Now} {Thread.CurrentThread.ManagedThreadId.ToString("D3")} {message}");
        }
        #endregion
    }
}