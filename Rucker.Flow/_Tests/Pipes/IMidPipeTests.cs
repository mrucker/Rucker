using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Rucker.Extensions;
using NUnit.Framework;
using Rucker.Flow._Tests.Classes.Mappers;

namespace Rucker.Flow.Tests
{
    [TestFixture("LambdaPipe")]
    [TestFixture("AsyncPipe")]
    [TestFixture("MapPipe")]
    [SuppressMessage("ReSharper", "HeuristicUnreachableCode")]    
    public class IMidPipeTests
    {
        #region Fields
        private readonly Func<Func<IEnumerable<string>>, IMidPipe<string, string>> _pipeFactory;
        #endregion

        #region Constructors
        public IMidPipeTests(string pipeType)
        {
            if (pipeType == "LambdaPipe")
            {
                _pipeFactory = consumes => new LambdaMidPipe<string, string>(items => items.Select(i => i.ToLower())) { Consumes = consumes() };
            }

            if (pipeType == "AsyncPipe")
            {
                //it is the "Async" command at the end of this line that makes this an "Asynchronous" mid pipe.
                _pipeFactory = consumes => new LambdaMidPipe<string, string>(items => items.Select(i => i.ToLower())) { Consumes = consumes() }.Async();
            }

            if (pipeType == "MapPipe")
            {
                _pipeFactory = consumes => new MapPipe<string, string>(new MapToLower()) { Consumes = consumes() }.Async();
            }


        }
        #endregion

        [Test]
        public void ValuesProducedTest()
        {
            var pipe = _pipeFactory(ManyProduction());

            Assert.AreEqual(PipeStatus.Created, pipe.Status);

            Assert.IsTrue(Production().Select(i => i.ToLower()).SequenceEqual(pipe.Produces));
        }

        [Test]
        public void ValuesProducedTwiceTest()
        {
            var pipe = _pipeFactory(ManyProduction());

            Assert.AreEqual(PipeStatus.Created, pipe.Status);

            Assert.IsTrue(Production().Select(i => i.ToLower()).SequenceEqual(pipe.Produces));

            Assert.IsTrue(Production().Select(i => i.ToLower()).SequenceEqual(pipe.Produces));
        }

        [Test]
        public void ValuesEmptiedTest()
        {
            var pipe = _pipeFactory(SingleProduction());

            Assert.AreEqual(PipeStatus.Created, pipe.Status);

            Assert.IsTrue(Production().Select(i => i.ToLower()).SequenceEqual(pipe.Produces));

            Assert.IsTrue(pipe.Produces.None());
        }

        [Test]
        public void FirstErrorTest()
        {
            var pipe = _pipeFactory(FirstError);

            Assert.That(pipe.Produces.ToArray, Throws.Exception.Message.EqualTo("First").Or.InnerException.Message.EqualTo("First"));

            Assert.AreEqual(PipeStatus.Errored, pipe.Status);
        }

        [Test]
        public void LastErrorTest()
        {
            var pipe = _pipeFactory(LastError);

            Assert.That(pipe.Produces.ToArray, Throws.Exception.Message.EqualTo("Last").Or.InnerException.Message.EqualTo("Last"));

            Assert.AreEqual(PipeStatus.Errored, pipe.Status);
        }

        [Test, Ignore("An interesting fail case. I don't think there is anything to be done.")]
        public void OnlyErrorTest()
        {
            var pipe = _pipeFactory(OnlyError);

            Assert.That(pipe.Produces.ToArray, Throws.Exception.Message.EqualTo("Only").Or.InnerException.Message.EqualTo("Only"));

            Assert.AreEqual(PipeStatus.Errored, pipe.Status);
        }

        [Test]
        public void CreatedStatusTest()
        {
            var pipe = _pipeFactory(SingleProduction());            

            Assert.AreEqual(PipeStatus.Created, pipe.Status);
        }

        [Test]
        public void WorkingStatusTest()
        {
            var pipe = _pipeFactory(SingleProduction());
            var prod = pipe.Produces.GetEnumerator();

            prod.MoveNext();

            Assert.AreEqual(PipeStatus.Working, pipe.Status);

            prod.MoveNext();

            Assert.AreEqual(PipeStatus.Working, pipe.Status);
        }

        [Test]
        public void FinishedStatusTest()
        {
            var pipe = _pipeFactory(SingleProduction());
            var prod = pipe.Produces.GetEnumerator();

            while(prod.MoveNext()) { }
            
            Assert.AreEqual(PipeStatus.Finished, pipe.Status);
        }

        [Test]
        public void EveryStatusTest()
        {
            var pipe = _pipeFactory(SingleProduction());
            var prod = pipe.Produces.GetEnumerator();

            Assert.AreEqual(PipeStatus.Created, pipe.Status);

            while (prod.MoveNext())
            {
                Assert.AreEqual(PipeStatus.Working, pipe.Status);
            }

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