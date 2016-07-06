using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Rucker.Extensions;
using NUnit.Framework;

namespace Rucker.Flow.Tests
{
    [TestFixture("PollPipe")]
    [TestFixture("ReadPipe")]
    [TestFixture("AsyncPipe")]
    [TestFixture("LambdaPipe")]
    [TestFixture("ThreadPipe(1)")]
    [TestFixture("ThreadPipe(2)")] //I assume if we can do two then we can do any number. Proof by mathematical induction? :)  
    [TestFixture("EnumerablePipe")]
    [SuppressMessage("ReSharper", "HeuristicUnreachableCode")]    
    [SuppressMessage("ReSharper", "ReturnValueOfPureMethodIsNotUsed")]
    public class IFirstPipeTests
    {
        #region Fields
        private readonly Func<Func<IEnumerable<string>>, IFirstPipe<string>> _pipeFactory;
        #endregion

        #region Constructors
        public IFirstPipeTests(string pipeType)
        {
            if (pipeType == "PollPipe")
            {
                _pipeFactory = production => new LambdaFirstPipe<string>(production).Poll(TimeSpan.Zero, new PollLimit {PollCount = 1});
            }

            if (pipeType == "ReadPipe")
            {
                _pipeFactory = production => new ReadPipe<string>(new ReadFunc(production), 1);
            }

            if (pipeType == "AsyncPipe")
            {
                _pipeFactory = production => new LambdaFirstPipe<string>(production).Async();
            }

            if (pipeType == "LambdaPipe")
            {
                _pipeFactory = production => new LambdaFirstPipe<string>(production);
            }

            if (pipeType == "ThreadPipe(1)")
            {
                _pipeFactory = production => new LambdaFirstPipe<string>(production).Thread(1);
            }

            if (pipeType == "ThreadPipe(2)")
            {
                _pipeFactory = production => new LambdaFirstPipe<string>(production).Thread(2);
            }

            if (pipeType == "EnumerablePipe")
            {
                _pipeFactory = production => new EnumerablePipe<string>(production());
            }

            if (_pipeFactory == null)
            {
                throw new ArgumentException($"Undefined pipeType ({pipeType})", nameof(pipeType));
            }
        }
        #endregion

        [Test]
        public void ValuesProducedTest()
        {
            var pipe = _pipeFactory(ManyProduction());

            Assert.IsTrue(Production().SequenceEqual(pipe.Produces));
        }

        [Test]
        public void ValuesProducedTwiceTest()
        {            
            var pipe = _pipeFactory(ManyProduction());

            if (pipe is ReadPipe<string>)
            {
                throw new IgnoreException("Because ReadPipe was built to work with an old framework that doesn't allow infinite reads it doesn't work for this test");
            }            

            Assert.IsTrue(Production().SequenceEqual(pipe.Produces));

            Assert.IsTrue(Production().SequenceEqual(pipe.Produces));
        }

        [Test]
        public void ValuesProducedOnceTest()
        {
            var pipe = _pipeFactory(SingleProduction());            

            Assert.IsTrue(Production().SequenceEqual(pipe.Produces));

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

        [Test]
        public void OnlyErrorTest()
        {
            var pipe = null as IFirstPipe<string>;

            try
            {
                pipe = _pipeFactory(OnlyError);
            }
            catch (Exception)
            {
                if (_pipeFactory(Production) is EnumerablePipe<string>)
                {
                    throw new IgnoreException("Because EnumerablePipe resolves the iterator block immediately the exception is thrown during the creation rather than while enumerating over the block");
                }
                throw;
            }

            Assert.That(pipe.Produces.ToArray, Throws.Exception.Message.EqualTo("Only").Or.InnerException.Message.EqualTo("Only"));

            Assert.AreEqual(PipeStatus.Errored, pipe.Status);
        }

        [Test]
        public void StopErrorTest()
        {
            var pipe = _pipeFactory(ManyProduction());

            pipe.Stop();

            Assert.That(pipe.Produces.ToArray, Throws.Exception.Message.Contain("stopped").Or.InnerException.Message.Contain("stopped"));
        }

        [Test]
        public void StopStatusTest()
        {
            var pipe = _pipeFactory(ManyProduction());

            var prod = pipe.Produces.GetEnumerator();

            prod.MoveNext();

            Assert.AreEqual(Production().First(), prod.Current);

            pipe.Stop();

            while (prod.MoveNext()) { }

            Assert.AreEqual(PipeStatus.Stopped, pipe.Status);
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

            while (prod.MoveNext()) { }

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