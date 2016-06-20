using System;
using Rucker.Testing;
using NUnit.Framework;
using Rucker.Data;


namespace Rucker.Flow.Tests
{
    [TestFixture]
    public class MultiStepTests
    {
        [Test]
        public void MultiStepRunsEachStepsOnlyOnce()
        {
            var step1 = new EtlCodeStep<IRows, IRows> { Reader = new ReadDelay(250), Writer = new WriteDelay(250) };
            var step2 = new EtlCodeStep<IRows, IRows> { Reader = new ReadDelay(250), Writer = new WriteDelay(250) };

            using (var step = new MultiStep(new[] { step1, step2 }))
                {
                    var expExecutionTime = new TimeSpan(0, 0, 0, 1);
                    var actExecutionTime = Test.ExecutionTime(step.Process);

                    Assert.That(actExecutionTime, Is.EqualTo(expExecutionTime).Within(250).Milliseconds);
            }
        }
    }
}