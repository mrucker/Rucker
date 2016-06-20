using System;

namespace Rucker.Flow.Tests
{
    public class FailProcessingStep: Step
    {
        protected override void Initializing()
        {

        }

        protected override void Processing()
        {
            throw new Exception();
        }
    }
}