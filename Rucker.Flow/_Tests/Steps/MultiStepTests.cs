using System;
using Rucker.Testing;
using NUnit.Framework;


namespace Rucker.Flow.Tests
{
    [TestFixture]
    public class MultiStepTests
    {
        [Test]
        public void MultiStepRunsEachStepsOnlyOnce()
        {
                using (var step = new MultiStep(new[] { new DelayedEtlCodeStep(500), new DelayedEtlCodeStep(500) }))
                {
                    var expExecutionTime = new TimeSpan(0, 0, 0, 1);
                    var actExecutionTime = Test.ExecutionTime(step.Process);

                    Assert.That(actExecutionTime, Is.EqualTo(expExecutionTime).Within(250).Milliseconds);
            }
        }
    }
}