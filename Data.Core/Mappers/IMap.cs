namespace Data.Core
{
    /// <summary>
    /// Able to map a Source Type to a Dest Type
    /// </summary>
    /// <typeparam name="TSource">Source Type</typeparam>
    /// <typeparam name="TDest">Dest Type</typeparam>
    public interface IMap<in TSource, out TDest>
    {
        /// <summary>
        /// Maps a Source Type to a Dest Type
        /// </summary>
        /// <param name="page">Source data</param>
        /// <returns>Mapped Dest</returns>
        TDest Map(TSource page);
    }

    public interface IMap<T> : IMap<T, T>
    { }
}