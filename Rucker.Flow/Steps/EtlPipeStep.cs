using System.Linq;

namespace Rucker.Flow
{
    public class EtlPipeStep<TSource, TDest>: EtlCodeStep<TSource, TDest>
    {
        protected override void Processing()
        {            
            var readPipe  = new ReadPipe<TSource>(Reader, Setting.MaxPageSize);
            var mapPipe   = new MapPipe<TSource, TDest>(Mappers.ToArray());
            var writePipe = new WritePipe<TDest>(Writer);

            using (Tracker.Whole(1, ToString()))
            {
                readPipe.Then(mapPipe).Then(writePipe).Thread(Setting.MaxDegreeOfParallelism).Start();
            }
        }
    }
}