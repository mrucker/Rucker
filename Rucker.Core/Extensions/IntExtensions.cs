using System;
using System.Linq;
using System.Collections.Generic;

namespace Rucker.Extensions
{
    public static class IntExtensions
    {
        public static bool Between(this int value, int lower, int upper, bool inclusive = true)
        {
            if(lower > upper) throw new ArgumentOutOfRangeException(nameof(lower));

            return inclusive ? lower <= value && value <= upper : lower < value && value < upper;
        }

        public static string Cat(this IEnumerable<int> values, string seperator = "")
        {
            return values.Select(v => v.ToString()).Cat(seperator);
        }
    }
}