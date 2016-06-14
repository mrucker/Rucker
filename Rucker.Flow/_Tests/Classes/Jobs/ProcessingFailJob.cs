using System;
using Rucker.Flow;

namespace Rucker.Flow.Tests.Classes
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