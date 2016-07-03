using NUnit.Framework;

namespace Rucker.Flow.Tests
{
    [TestFixture]
    public class LambdaFirstPipeTests: IFirstPipeTests
    {
        public LambdaFirstPipeTests(): base(production => new LambdaFirstPipe<string>(production))
        { }
    }
}