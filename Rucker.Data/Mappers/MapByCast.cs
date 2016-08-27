namespace Rucker.Data
{
    public class MapByCast<TSource, TDest>: IMap<TSource, TDest>
    {
        public TDest Map(TSource page)
        {
            return (TDest)(object)page;
        }
    }    
}