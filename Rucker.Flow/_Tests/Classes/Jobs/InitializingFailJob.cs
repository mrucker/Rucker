using System;
using Rucker.Flow;

namespace Rucker.Flow.Tests.Classes
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