using System;

namespace Rucker.Flow.Tests
{
    public class InitializingFailJob: Job
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