using System.Security;
using NUnit.Framework;

namespace Rucker.Core.Tests
{
    [TestFixture]
    public class ConfirmTests
    {
        [Test]
        public void ConfirmThatReturnsCorrectConfirmation()
        {
            Assert.IsTrue(Confirm.That(true).Success);
            Assert.IsFalse(Confirm.That(false).Success);
        }

        [Test]
        public void ConfirmThatOrReturnsCorrectConfirmation()
        {
            Assert.IsTrue(Confirm.That(true).Or(false).Success);
            Assert.IsTrue(Confirm.That(false).Or(true).Success);
            Assert.IsTrue(Confirm.That(true).Or(true).Success);
            Assert.IsFalse(Confirm.That(false).Or(false).Success);
        }

        [Test]
        public void ElseThrowsDoesThrow()
        {
            Assert.Throws(Is.TypeOf<SecurityException>().And.Message.EqualTo("Failure"), () => new Confirm.Confirmation {Success = false}.ElseThrow("Failure"));            
        }

        [Test]
        public void ElseThrowsDoesNotThrow()
        {
            Assert.DoesNotThrow(() => new Confirm.Confirmation { Success = true }.ElseThrow("Failure"));
        }
    }
}