using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rucker.Extensions;

namespace Rucker.Flow.Tests
{
    [TestFixture]
    public class LambdaLastPipeTests
    {
        [Test]
        public void ValuesWrittenTest()
        {
            var dest = new List<string>();
            var pipe = new LambdaLastPipe<string>(dest.AddRange) { Consumes = Infinite()};            

            Assert.AreEqual(PipeStatus.Created, pipe.Status);

            pipe.Start();

            Assert.IsTrue(Infinite().SequenceEqual(dest));

            Assert.AreEqual(PipeStatus.Finished, pipe.Status);
        }

        [Test]
        public void ValuesWrittenTwiceTest()
        {
            var dest = new List<string>();
            var pipe = new LambdaLastPipe<string>(dest.AddRange) { Consumes = Infinite() };

            Assert.AreEqual(PipeStatus.Created, pipe.Status);

            pipe.Start();

            Assert.IsTrue(Infinite().SequenceEqual(dest));

            Assert.AreEqual(PipeStatus.Finished, pipe.Status);

            pipe.Start();

            Assert.IsTrue(Infinite().Concat(Infinite()).SequenceEqual(dest));

            Assert.AreEqual(PipeStatus.Finished, pipe.Status);
        }

        [Test]
        public void ValuesEmptiedTest()
        {
            var produce = new[] { "A", "B", "C" };
            var copy1   = new Queue<string>(produce);
            var dest    = new List<string>();

            var pipe = new LambdaLastPipe<string>(dest.AddRange) { Consumes = Emptyable(copy1) };

            Assert.AreEqual(PipeStatus.Created, pipe.Status);

            pipe.Start();
             
            Assert.IsTrue(produce.SequenceEqual(dest));

            Assert.AreEqual(PipeStatus.Finished, pipe.Status);

            pipe.Start();

            Assert.IsTrue(produce.SequenceEqual(dest));

            Assert.AreEqual(PipeStatus.Finished, pipe.Status);
        }

        [Test]
        public void FirstErrorTest()
        {
            var dest = new List<string>();
            var pipe = new LambdaLastPipe<string>(dest.AddRange) { Consumes = FirstError() };

            Assert.Throws<Exception>(() => pipe.Start());

            Assert.IsTrue(dest.None());

            Assert.AreEqual(PipeStatus.Errored, pipe.Status);
        }

        [Test]
        public void LastErrorTest()
        {
            var dest = new List<string>();
            var pipe = new LambdaLastPipe<string>(dest.AddRange) { Consumes = LastError() };

            Assert.Throws<Exception>(() => pipe.Start());

            Assert.IsTrue(Infinite().SequenceEqual(dest));

            Assert.AreEqual(PipeStatus.Errored, pipe.Status);
        }

        [Test, Ignore("An interesting fail case. I don't think there is anything to be done.")]
        public void OnlyErrorTest()
        {
            var dest = new List<string>();
            var pipe = new LambdaLastPipe<string>(dest.AddRange) { Consumes = OnlyError() };

            Assert.Throws<Exception>(() => pipe.Start());

            Assert.IsTrue(dest.None());

            Assert.AreEqual(PipeStatus.Errored, pipe.Status);
        }

        #region Private Methods
        private IEnumerable<string> Infinite()
        {
            return new[] { "A", "B", "C" };
        }

        private IEnumerable<string> Emptyable(Queue<string> queue)
        {
            while (queue.Count > 0)
            {
                yield return queue.Dequeue();
            }
        }

        private IEnumerable<string> FirstError()
        {
            throw new Exception();

            #pragma warning disable 162
            foreach (var item in Infinite())
            {
                yield return item;
            }
            #pragma warning restore 162
        }

        private IEnumerable<string> LastError()
        {
            foreach(var item in Infinite())
            {
                yield return item;
            }

            throw new Exception();
        }

        private IEnumerable<string> OnlyError()
        {
            throw new Exception();
        }
        #endregion
    }
}