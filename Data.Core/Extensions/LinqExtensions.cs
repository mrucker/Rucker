using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace Data.Core
{
    public static class LinqExtensions
    {
        public static IQueryable Take(this IQueryable source, int count)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            return source.Provider.CreateQuery(Expression.Call(typeof(Queryable), "Take", new [] { source.ElementType }, source.Expression, Expression.Constant(count)));
        }

        public static bool IsNullOrNone<TSource>(this IEnumerable<TSource> source)
        {
            return source == null || source.None();
        }

        public static bool None<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            return !source.Any(predicate);
        }

        public static bool None<TSource>(this IEnumerable<TSource> source)
        {
            return !source.Any();
        }

        public static bool Many<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            return source.Count(predicate) > 1;
        }

        public static bool Many<TSource>(this IEnumerable<TSource> source)
        {
            return source.Count() > 1;
        }

        public static bool Missing<TSource>(this IEnumerable<TSource> source, TSource value)
        {
            return !source.Contains(value);
        }

        public static bool One<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            return source.SingleOrDefault(predicate) != null;
        }

        public static bool One<TSource>(this IEnumerable<TSource> source)
        {
            return source.SingleOrDefault() != null;
        }

        public static bool None<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            return !source.Any(predicate);
        }

        public static bool None<TSource>(this IQueryable<TSource> source)
        {
            return !source.Any();
        }

        public static bool Many<TSource>(this IQueryable<TSource> source, Func<TSource, bool> predicate)
        {
            return source.Count(predicate) > 1;
        }

        public static bool Many<TSource>(this IQueryable<TSource> source)
        {
            return source.Count() > 1;
        }

        public static bool One<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            return source.SingleOrDefault(predicate) != null;
        }

        public static bool One<TSource>(this IQueryable<TSource> source)
        {
            return source.SingleOrDefault() != null;
        }
    }
}