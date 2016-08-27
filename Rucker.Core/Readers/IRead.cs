using System;
using System.Collections.Generic;

namespace Rucker.Core
{
    /// <summary>
    /// Able to read data
    /// </summary>
    /// <typeparam name="T">Type to read</typeparam>
    public interface IRead<out T>: IDisposable
    {
        /// <summary>
        /// Entire count of entries that can be read
        /// </summary>
        /// <returns></returns>
        int Size();

        /// <summary>
        /// Read a page of data (if skip and take are 0 then read everything)
        /// </summary>
        /// <param name="skip">Number of entries to skip</param>
        /// <param name="take">Number of entries to take</param>
        /// <returns>Page of entries starting at skip and proceeding to (skip + take).</returns>
        /// <remarks>Read(0, Size()) should return everything</remarks>
        T Read(int skip, int take);
    }

    public static class IReadExtensions
    {
        public static T ReadAll<T>(this IRead<T> reader)
        {
            return reader.Read(0, reader.Size());
        }

        public static IEnumerable<T> Read<T>(this IRead<IRows> reader)
        {
            return BaseRows.RowsToObjects<T>(reader.ReadAll());
        }

        public static IEnumerable<T> Read<T>(this IRead<IRows> reader, int skip, int take)
        {
            return BaseRows.RowsToObjects<T>(reader.Read(skip, take));
        }

        public static int PageCount<T>(this IRead<T> reader, int pageSize)
        {
            return pageSize == -1 ? 1 : (int)Math.Ceiling((double)reader.Size()/pageSize);
        }
    }
}
