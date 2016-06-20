using System;

namespace Rucker.Flow.Tests
{
    public class ProcessingFailStep: Step
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