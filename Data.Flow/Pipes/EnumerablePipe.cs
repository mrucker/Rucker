using System.Collections.Generic;

namespace Data.Flow
{
    public class EnumerablePipe<T>: LambdaFirstPipe<T>
    {
        public EnumerablePipe(IEnumerable<T> enumerable) : base(() => enumerable)
        { }
    }
}