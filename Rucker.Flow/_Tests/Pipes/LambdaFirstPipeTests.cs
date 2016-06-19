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
    public class LambdaFirstPipeTests
    {
        [Test]
        public void ValuesProducedTest()
        {
            var pipe    = new LambdaFirstPipe<string>(InfiniteProduction);

            Assert.AreEqual(PipeStatus.Created, pipe.Status);

            Assert.IsTrue(InfiniteProduction().SequenceEqual(pipe.Produces));

            Assert.AreEqual(PipeStatus.Finished, pipe.Status);
        }

        [Test]
        public void ValuesProducedTwiceTest()
        {
            var pipe = new LambdaFirstPipe<string>(InfiniteProduction);

            Assert.AreEqual(PipeStatus.Created, pipe.Status);

            Assert.IsTrue(InfiniteProduction().SequenceEqual(pipe.Produces));

            Assert.AreEqual(PipeStatus.Finished, pipe.Status);

            Assert.IsTrue(InfiniteProduction().SequenceEqual(pipe.Produces));

            Assert.AreEqual(PipeStatus.Finished, pipe.Status);
        }

        [Test]
        public void ValuesEmptiedTest()
        {
            var produce = new [] {"A", "B", "C"};
            var copy1 = new Queue<string>(produce);

            var pipe = new LambdaFirstPipe<string>(() => EmptyableProduction(copy1));

            Assert.AreEqual(PipeStatus.Created, pipe.Status);

            Assert.IsTrue(produce.SequenceEqual(pipe.Produces));

            Assert.AreEqual(PipeStatus.Finished, pipe.Status);

            Assert.IsTrue(pipe.Produces.None());

            Assert.AreEqual(PipeStatus.Finished, pipe.Status);
        }

        [Test]
        public void FirstErrorTest()
        {            
            var pipe = new LambdaFirstPipe<string>(FirstErrorProduction);

            Assert.Throws<Exception>(() => pipe.Produces.ToArray());

            Assert.AreEqual(PipeStatus.Errored, pipe.Status);
        }

        [Test]
        public void LastErrorTest()
        {
            var pipe = new LambdaFirstPipe<string>(LastErrorProduction);

            Assert.Throws<Exception>(() => pipe.Produces.ToArray());

            Assert.AreEqual(PipeStatus.Errored, pipe.Status);
        }

        [Test]
        public void OnlyErrorTest()
        {
            var pipe = new LambdaFirstPipe<string>(OnlyErrorProduction);

            Assert.Throws<Exception>(() => pipe.Produces.ToArray());

            Assert.AreEqual(PipeStatus.Errored, pipe.Status);
        }

        [Test]
        public void WorkingStatusTest()
        {
            var produce = new[] { "A", "B", "C" };
            var copy1 = new Queue<string>(produce);

            var pipe = new LambdaFirstPipe<string>(() => EmptyableProduction(copy1));
            var prod = pipe.Produces.GetEnumerator();

            Assert.AreEqual(PipeStatus.Created, pipe.Status);

            prod.MoveNext();

            Assert.AreEqual(produce.First(), prod.Current);

            Assert.AreEqual(PipeStatus.Working, pipe.Status);

            prod.MoveNext();

            Assert.AreEqual(produce.Skip(1).First(), prod.Current);

            Assert.AreEqual(PipeStatus.Working, pipe.Status);
        }

        [Test]
        public void StopStatusTest()
        {
            var pipe = new LambdaFirstPipe<string>(InfiniteProduction);
            var prod = pipe.Produces.GetEnumerator();

            Assert.AreEqual(PipeStatus.Created, pipe.Status);

            prod.MoveNext();

            Assert.AreEqual(InfiniteProduction().First(), prod.Current);

            Assert.AreEqual(PipeStatus.Working, pipe.Status);

            pipe.Stop();

            Assert.IsFalse(prod.MoveNext());

            Assert.AreEqual(PipeStatus.Stopped, pipe.Status);
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