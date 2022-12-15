using System.Linq;

namespace Data.Flow
{
    public class EtlPipeStep<TSource, TDest>: EtlCodeStep<TSource, TDest>
    {
        protected override void Processing()
        {            
            var readPipe  = new ReadPipe<TSource>(Reader, Setting.MaxPageSize);
            var mapPipe   = new MapPipe<TSource, TDest>(Mappers.ToArray());
            var writePipe = new WritePipe<TDest>(Writer);
            var wholePipe = readPipe.Then(mapPipe).Then(writePipe).Thread(Setting.MaxDegreeOfParallelism);

            using (Tracker.Whole(1, ToString()))
            {
                wholePipe.Start();
                wholePipe.Dispose();
            }
        }

        public override string ToString()
        {
            return base.ToString().Replace("EtlPipeStep`2", "EtlPipeStep");
        }
    }
}