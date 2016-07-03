namespace Rucker.Flow.Tests
{
    public class AsyncPipeTests: IFirstPipeTests
    {
        public AsyncPipeTests(): base(production => new LambdaFirstPipe<string>(production).Async())
        { }
    }
}