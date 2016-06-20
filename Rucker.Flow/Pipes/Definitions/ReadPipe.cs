using System.Collections.Generic;
using Rucker.Data;

namespace Rucker.Flow
{
    public class ReadPipe<T>: LambdaFirstPipe<T>
    {
        public ReadPipe(IRead<T> reader, int pageSize): base(() => Read(reader, pageSize))
        { }

        public static IEnumerable<T> Read(IRead<T> reader, int pageSize)
        {
            var pageCount = reader.PageCount(pageSize);            

            for (var i = 0; i < pageCount; i++)
            {
                if (pageSize == -1)
                {
                    yield return reader.ReadAll();
                }
                else
                {
                    yield return reader.Read(i * pageSize, pageSize);
                }
            }
        }
    }
}