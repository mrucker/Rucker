using NUnit.Framework;
using Rucker.Core;


namespace Rucker.Flow.Tests
{
    [TestFixture]
    public class EtlPipeStepTests: EtlCodeStepTests
    {
        public EtlPipeStepTests() : base(() => new EtlPipeStep<IRows, IRows>())
        {
            
        }
    }
}