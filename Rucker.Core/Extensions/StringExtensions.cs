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

        public static BaseUri ToUri(this string uriString)
        {
            var uri = new BaseUri(uriString);

            if (uri.Scheme.ToLower() == "table")
            {
                return new TableUri(uriString);
            }

            if (uri.Scheme.ToLower() == "database")
            {
                return new DatabaseUri(uriString);
            }

            if (uri.Scheme.ToLower() == "file")
            {
                return new FileUri(uriString);
            }

            return uri;
        }

        public static T ToUri<T>(this string uriString) where T : BaseUri
        {
            if (uriString == null)
            {
                return null;
            }

            if (typeof(T) == typeof(TableUri))
            {
                return new TableUri(uriString) as T;
            }

            if (typeof(T) == typeof(DatabaseUri))
            {
                return new DatabaseUri(uriString) as T;
            }

            if (typeof(T) == typeof(FileUri))
            {
                return new FileUri(uriString) as T;
            }

            if (typeof(T) == typeof(DirectoryUri))
            {
                return new DirectoryUri(uriString) as T;
            }

            throw new Exception("Not a recogiznied Uri type");
        }
    }
}