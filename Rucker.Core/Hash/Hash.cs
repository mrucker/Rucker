using System.Linq;

namespace Rucker.Core
{
    public static class Hash
    {
        /// <remarks>
        /// Taken from http://stackoverflow.com/a/892640
        /// </remarks>
        public static int GetHashCode(params object[] objs)
        {
            unchecked { return objs.Select(o => o?.GetHashCode() ?? 0).Aggregate(23, (c, h) => c * 31 + h); }
        }
    }
}