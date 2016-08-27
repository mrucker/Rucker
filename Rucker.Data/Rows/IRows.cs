using System;
using System.Linq;
using System.Collections.Generic;
using Rucker.Core;

namespace Rucker.Data
{
    public interface IRows: IEnumerable<IRow>
    {
    }

    public static class IRowsExtensions
    {
        public static IEnumerable<string> Columns(this IEnumerable<IRow> rows)
        {
            return rows.First().Columns;
        }

        public static IRows ToRows(this IEnumerable<IRow> rows)
        {
            return (rows as BaseRows) ?? new BaseRows(rows);
        }

        public static IEnumerable<T> To<T>(this IEnumerable<IRow> rows)
        {
            return BaseRows.RowsToObjects<T>(rows);
        }

        public static IRows Union(this IEnumerable<IRow> rows, IRow row)
        {
            return rows.Select(r => r.Union(row)).ToRows();
        }

        public static IRows WithoutColumns(this IEnumerable<IRow> rows, IEnumerable<string> withoutColumns)
        {
            withoutColumns = withoutColumns.ToArray();
            
            return rows.Select(r => new WithoutColumnsRow(r, withoutColumns)).ToRows();
        }

        public static IRows WithColumns(this IEnumerable<IRow> rows, IEnumerable<string> withColumns)
        {
            withColumns = withColumns.ToArray();
            rows        = rows.ToArray();

            var missingWithColumns = withColumns.Except(rows.First().Columns).ToArray();

            if (missingWithColumns.Any())
            {
                throw new UserMessageException($"Expected columns ({missingWithColumns.Cat(", ")}) were missing");
            }

            var withoutColumns = rows.First().Columns.Except(withColumns).ToArray();

            return rows.Select(r => new WithoutColumnsRow(r, withoutColumns)).ToRows();
        }

        public static IRows WithDistinct(this IEnumerable<IRow> rows, IEnumerable<string> cols)
        {
            const int pageSize = 100000;

            rows = rows.ToArray();
            cols = cols.ToArray();

            if (cols.None())
            {
                return rows.ToRows();
            }

            if (rows.Count() > pageSize)
            {
                //The default .Distinct() algorithm slows down considerably for collections larger than 100,000 entries so we page
                return rows.WithDistinctPages(cols, pageSize).SelectMany(p => p.ToRows()).Distinct(new EqualColumnValues(cols)).ToRows();
            }

            return rows.Distinct(new EqualColumnValues(cols)).ToRows();            
        }

        public static IRows Rename(this IEnumerable<IRow> rows, string oldColumn, string newColumn)
        {
            rows = rows.ToRows();

            foreach (var row in rows)
            {
                row.Rename(oldColumn, newColumn);
            }

            return rows.ToRows();
        }

        #region Private Methods
        private static IEnumerable<IEnumerable<IRow>> WithDistinctPages(this IEnumerable<IRow> rows, IEnumerable<string> cols, int pageSize)
        {
            cols = cols.ToArray();
            rows = rows.ToArray();

            var pageCount = Math.Ceiling((decimal) rows.Count()/pageSize);

            for (var i = 0; i < pageCount; i++)
            {
                yield return rows.Skip(i * pageSize).Take(pageSize).Distinct(new EqualColumnValues(cols)).ToRows();
            }
        }
        #endregion
    }
}