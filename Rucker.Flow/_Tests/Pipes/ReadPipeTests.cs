
namespace Rucker.Flow.Tests
{
    public class ReadPipeTests: IFirstPipeTests
    {
        public ReadPipeTests() : base(production => new ReadPipe<string>(new ReadFunc(production), 1))
        { }
    }
}