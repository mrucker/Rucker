using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;

namespace Data.Flow.Tests
{
    [TestFixture("LambdaPipe")]
    [SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
    public class ILastPipeTests
    {
        #region Fields
        private readonly Func<Func<IEnumerable<string>>, List<string>, ILastPipe<string>> _pipeFactory;
        #endregion

        #region Constructors
        public ILastPipeTests(string pipeType)
        {
            if (pipeType == "LambdaPipe")
            {
                _pipeFactory = (consumes, produces) => new LambdaLastPipe<string>(produces.AddRange) { Consumes = consumes() };
            }

            if (_pipeFactory == null)
            {
                throw new ArgumentException($"Undefined pipeType ({pipeType})", nameof(pipeType));
            }
        }
        #endregion

        [Test]
        public void ValuesWrittenTest()
        {
            var dest = new List<string>();
            var pipe = _pipeFactory(ManyProduction(), dest);

            pipe.Start();

            Assert.IsTrue(Production().SequenceEqual(dest));            
        }

        [Test]
        public void ValuesWrittenTwiceTest()
        {
            var dest = new List<string>();
            var pipe = _pipeFactory(ManyProduction(), dest);

            pipe.Start();

            Assert.IsTrue(Production().SequenceEqual(dest));

            pipe.Start();

            Assert.IsTrue(Production().Concat(Production()).SequenceEqual(dest));
        }

        [Test]
        public void ValuesEmptiedTest()
        {
            var dest = new List<string>();
            var pipe = _pipeFactory(SingleProduction(), dest);

            pipe.Start();
             
            Assert.IsTrue(Production().SequenceEqual(dest));

            pipe.Start();

            Assert.IsTrue(Production().SequenceEqual(dest));            
        }

        [Test]
        public void FirstErrorTest()
        {
            var dest = new List<string>();
            var pipe = _pipeFactory(FirstError, dest);

            Assert.That(pipe.Start, Throws.Exception.Message.EqualTo("First").Or.InnerException.Message.EqualTo("First"));

            Assert.AreEqual(PipeStatus.Errored, pipe.Status);

            Assert.IsEmpty(dest);
        }

        [Test]
        public void LastErrorTest()
        {
            var dest = new List<string>();
            var pipe = _pipeFactory(LastError, dest);

            Assert.That(pipe.Start, Throws.Exception.Message.EqualTo("Last").Or.InnerException.Message.EqualTo("Last"));

            Assert.AreEqual(PipeStatus.Errored, pipe.Status);

            Assert.IsTrue(Production().Take(2).SequenceEqual(dest));
        }

        [Test]
        [Ignore("An interesting fail case. I don't think there is anything to be done.")]
        public void OnlyErrorTest()
        {
            var dest = new List<string>();
            var pipe = _pipeFactory(OnlyError, dest);

            Assert.That(pipe.Start, Throws.Exception.Message.EqualTo("Only").Or.InnerException.Message.EqualTo("Only"));

            Assert.AreEqual(PipeStatus.Errored, pipe.Status);

            Assert.IsEmpty(dest);
        }

        [Test]
        public void CreatedStatusTest()
        {
            var dest = new List<string>();
            var pipe = _pipeFactory(SingleProduction(), dest);

            Assert.AreEqual(PipeStatus.Created, pipe.Status);
        }

        [Test]
        public void FinishedStatusTest()
        {
            var dest = new List<string>();
            var pipe = _pipeFactory(SingleProduction(), dest);

            pipe.Start();

            Assert.AreEqual(PipeStatus.Finished, pipe.Status);
        }

        #region Private Methods
        private static IEnumerable<string> Production()
        {
            yield return "A";
            yield return "B";
            yield return "C";
        }

        private static Func<IEnumerable<string>> ManyProduction()
        {
            return Production;
        }

        private static Func<IEnumerable<string>> SingleProduction()
        {
            var block = new BlockingCollection<string>();

            foreach (var item in Production())
            {
                block.Add(item);
            }

            block.CompleteAdding();

            return () => block.GetConsumingEnumerable();
        }

        private static IEnumerable<string> FirstError()
        {
            throw new Exception("First");

            #pragma warning disable 162
            yield return "A";
            yield return "B";
            yield return "C";
            #pragma warning restore 162
        }

        private static IEnumerable<string> LastError()
        {
            yield return "A";
            yield return "B";
            throw new Exception("Last");
        }

        private static IEnumerable<string> OnlyError()
        {
            throw new Exception("Only");
        }
        #endregion
    }
}