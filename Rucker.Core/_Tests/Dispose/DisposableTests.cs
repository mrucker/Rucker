using System;
using NUnit.Framework;
using Rucker.Dispose;

namespace Rucker.Tests
{
    [TestFixture]
    public class DisposableTests
    {
        private class BadPattern : Disposable
        {
            protected override void Dispose(bool disposing)
            {
                //this sould call base.Dispose(disposing). Otherwise, other classes in the inheritance hierarchy might not have their resources disposed.
            }
        }

        private class GoodPattern : Disposable
        {
            public new void DisposeCheck()
            {
                base.DisposeCheck();
            }

            protected override void Dispose(bool disposing)
            {
                if (!disposing)
                {
                    throw new Exception("Dispose called but disposing is false. Something is wrong.");
                }

                base.Dispose(true);
            }
        }

        [Test]
        public void BadPatternThrowsException()
        {
            var badPattern = new BadPattern();

            Assert.Throws<InvalidOperationException>(badPattern.Dispose);
        }

        [Test]
        public void GoodPatternThrowsNoException()
        {
            var goodPattern = new GoodPattern();

            Assert.DoesNotThrow(goodPattern.Dispose);
        }

        [Test]
        public void GoodPatternMultiDisposeThrowsNoException()
        {
            var goodPattern = new GoodPattern();

            Assert.DoesNotThrow(goodPattern.Dispose);
            Assert.DoesNotThrow(goodPattern.Dispose);
        }

        [Test]
        public void GoodPatternDisposeCheckPassesBeforeDispose()
        {
            var goodPattern = new GoodPattern();
            
            Assert.DoesNotThrow(goodPattern.DisposeCheck);
        }

        [Test]
        public void GoodPatternDisposeCheckFailsAfterDispose()
        {
            var goodPattern = new GoodPattern();

            goodPattern.Dispose();

            Assert.Throws<ObjectDisposedException>(goodPattern.DisposeCheck);            
        }
    }
}