using System;

namespace Rucker.Flow.Tests
{
    public class InitializingFailStep: Step
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