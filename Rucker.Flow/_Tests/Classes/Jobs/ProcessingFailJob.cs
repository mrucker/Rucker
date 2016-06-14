using System;

namespace Rucker.Flow.Tests
{
    public class ProcessingFailJob: Job
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