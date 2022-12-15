using System;

namespace Data.Flow.Tests
{
    public class FailInitializingStep: Step
    {
        protected override void Initializing()
        {
            throw new Exception();
        }

        protected override void Processing()
        {
            
        }
    }
}