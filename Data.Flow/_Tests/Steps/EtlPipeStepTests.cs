using NUnit.Framework;
using Data.Core;


namespace Data.Flow.Tests
{
    [TestFixture]
    public class EtlPipeStepTests: EtlCodeStepTests
    {
        public EtlPipeStepTests() : base(() => new EtlPipeStep<IRows, IRows>())
        {
            
        }
    }
}