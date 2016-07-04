using System.Collections.Generic;

namespace Rucker.Flow
{
    public class EnumerablePipe<T>: LambdaFirstPipe<T>
    {
        public EnumerablePipe(IEnumerable<T> enumerable) : base(() => enumerable)
        { }
    }
}