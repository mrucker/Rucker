using System;

namespace Data.Flow.Tests
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