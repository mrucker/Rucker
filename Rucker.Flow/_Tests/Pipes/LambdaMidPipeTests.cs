using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Rucker.Extensions;
using NUnit.Framework;

namespace Rucker.Flow.Tests
{

    [SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
    [TestFixture]
    public class LambdaMidPipeTests
    {
        [Test]
        public void ValuesProducedTest()
        {
            var pipe = new LambdaMidPipe<string, string>(items => items.Select(i => i.ToLower())) { Consumes = InfiniteProduction() };

            Assert.AreEqual(PipeStatus.Created, pipe.Status);

            Assert.IsTrue(InfiniteProduction().Select(i => i.ToLower()).SequenceEqual(pipe.Produces));

            Assert.AreEqual(PipeStatus.Finished, pipe.Status);
        }

        [Test]
        public void ValuesProducedTwiceTest()
        {
            var pipe = new LambdaMidPipe<string, string>(items => items.Select(i => i.ToLower())) { Consumes = InfiniteProduction() };

            Assert.AreEqual(PipeStatus.Created, pipe.Status);

            Assert.IsTrue(InfiniteProduction().Select(i => i.ToLower()).SequenceEqual(pipe.Produces));

            Assert.AreEqual(PipeStatus.Finished, pipe.Status);

            Assert.IsTrue(InfiniteProduction().Select(i => i.ToLower()).SequenceEqual(pipe.Produces));

            Assert.AreEqual(PipeStatus.Finished, pipe.Status);
        }

        [Test]
        public void ValuesEmptiedTest()
        {
            var produce = new [] {"A", "B", "C"};
            var copy1 = new Queue<string>(produce);

            var pipe = new LambdaMidPipe<string, string>(items => items.Select(i => i.ToLower())) { Consumes = EmptyableProduction(copy1) };                        

            Assert.AreEqual(PipeStatus.Created, pipe.Status);

            Assert.IsTrue(InfiniteProduction().Select(i => i.ToLower()).SequenceEqual(pipe.Produces));

            Assert.AreEqual(PipeStatus.Finished, pipe.Status);

            Assert.IsTrue(pipe.Produces.None());

            Assert.AreEqual(PipeStatus.Finished, pipe.Status);
        }

        [Test]
        public void FirstErrorTest()
        {
            var pipe = new LambdaMidPipe<string, string>(items => items.Select(i => i.ToLower())) { Consumes = FirstErrorProduction() };

            Assert.Throws<Exception>(() => pipe.Produces.ToArray());

            Assert.AreEqual(PipeStatus.Errored, pipe.Status);
        }

        [Test]
        public void LastErrorTest()
        {
            var pipe = new LambdaMidPipe<string, string>(items => items.Select(i => i.ToLower())) { Consumes = LastErrorProduction() };

            Assert.Throws<Exception>(() => pipe.Produces.ToArray());

            Assert.AreEqual(PipeStatus.Errored, pipe.Status);
        }

        [Test]
        public void WorkingStatusTest()
        {
            var produce = new[] { "A", "B", "C" };
            var copy1 = new Queue<string>(produce);

            var pipe = new LambdaMidPipe<string, string>(items => items.Select(i => i.ToLower())) { Consumes = EmptyableProduction(copy1) };
            var prod = pipe.Produces.GetEnumerator();

            Assert.AreEqual(PipeStatus.Created, pipe.Status);

            prod.MoveNext();

            Assert.AreEqual(produce.First().ToLower(), prod.Current);

            Assert.AreEqual(PipeStatus.Working, pipe.Status);

            prod.MoveNext();

            Assert.AreEqual(produce.Skip(1).First().ToLower(), prod.Current);

            Assert.AreEqual(PipeStatus.Working, pipe.Status);
        }

        #region Private Methods
        private IEnumerable<string> InfiniteProduction()
        {
            return new [] {"A", "B", "C"};
        }        

        private IEnumerable<string> EmptyableProduction(Queue<string> queue)
        {
            while (queue.Count > 0)
            {
                yield return queue.Dequeue();
            }
        }
        
        private IEnumerable<string> FirstErrorProduction()
        {
            throw new Exception();
            
            #pragma warning disable 162
            yield return "1";
            yield return "2;";
            #pragma warning restore 162
        }

        private IEnumerable<string> LastErrorProduction()
        {
            yield return "1";
            yield return "2;";
            throw new Exception();
        }

        private IEnumerable<string> OnlyErrorProduction()
        {
            throw new Exception();
        }
        #endregion
    }
}