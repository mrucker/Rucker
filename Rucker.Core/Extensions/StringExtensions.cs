using System;
using System.Linq;
using System.Collections.Generic;

namespace Rucker.Core
{
    public static class StringExtensions
    {
        public static string ToFirstUpper(this string value)
        {
            if(value == null) throw new ArgumentException(nameof(value));

            if (value.Length == 0) return value;
            if (value.Length == 1) return value.ToUpper();

            return char.ToUpper(value[0]) + value.Substring(1).ToLower();
        }

        public static bool IsSpaces(this string value)
        {
            return value != null && string.IsNullOrWhiteSpace(value);
        }

        public static bool IsNullOrSpaces(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static bool IsNotNullOrEmpty(this string value)
        {
            return !string.IsNullOrEmpty(value);
        }

        public static bool IsNotNullOrSpaces(this string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }        

        public static string Cat(this IEnumerable<string> values, string seperator = "")
        {
            return string.Join(seperator, values);
        }

        public static string Cat(this IEnumerable<char> chars)
        {
            return new string(chars.ToArray());
        }

        public static string EmptyOrTrim(this string value)
        {
            return value?.Trim() ?? "";
        }

        public static bool Missing(this string source, string value)
        {
            return !source.Contains(value);
        }
    }
}