using System;
using System.Text;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace Data.Core
{
    public abstract class BaseRow: IRow
    {
        #region ToRow
        public static IRow JsonToRow(string json)
        {
            var dictionary = json.Trim('{', '}').Trim().Split(',').Select(kv => kv.Split(':')).ToDictionary(kv => kv.Take(1).Single().Trim().Replace("\"", ""), kv => kv.Skip(1).Single().Trim().Replace("\"", ""));

            var definitions  = dictionary.Select(kv => new ObjectRow.PropertyDefinition(kv.Key, typeof(string)));
            var values       = dictionary.Select(kv => new ObjectRow.PropertyValue(kv.Key, kv.Value));

            return new ObjectRow(definitions, values);
        }

        public static IRow ObjectToRow(object @object)
        {
            return (@object.GetType().IsValueType || @object is string) ? new ValueRow(@object) : new ObjectRow(@object) as IRow;
        }

        public static IRow DataRowToRow(DataRow dataRow)
        {
            var columnNames = dataRow.Table.Columns.Cast<DataColumn>().Select(c => c.ColumnName).Distinct().ToArray();
            var definitions = columnNames.Select(n => new ObjectRow.PropertyDefinition(n, Type(dataRow, n))).ToArray();
            var values      = columnNames.Select(n => new ObjectRow.PropertyValue(n, Value(dataRow, n))).ToArray();

            return new ObjectRow(definitions, values);
        }

        public static IRow DataRowToRow(DataRow dataRow, IEnumerable<ObjectRow.PropertyDefinition> definitions)
        {
            definitions = definitions.ToArray();
            var values = definitions.Select(d => new ObjectRow.PropertyValue(d.Name, Value(dataRow, d.Name))).ToArray();

            return new ObjectRow(definitions, values);
        }

        public static IRow DataReaderToRow(IDataReader reader)
        {
            var definitions = new List<ObjectRow.PropertyDefinition>();
            var values      = new List<ObjectRow.PropertyValue>();

            for (var i = 0; i < reader.FieldCount; i++)
            {
                var column = reader.GetName(i);
                var value  = reader[i] is DBNull ? null : reader[i];
                var type   = reader.GetFieldType(i);

                definitions.Add(new ObjectRow.PropertyDefinition(column, type));
                values.Add(new ObjectRow.PropertyValue(column, value));
            }

            return new ObjectRow(definitions, values);
        }
        #endregion

        #region RowTo
        public static T RowToObject<T>(IRow row)
        {
            var tType = typeof(T);
            
            if ((tType.IsValueType || tType == typeof(string)))
            {
                return row.Values.Single().To<T>();
            }

            var tInstance   = (T)Activator.CreateInstance(tType);
            var tProperties = tType.GetProperties(BindingFlags.Instance | BindingFlags.Public).ToDictionary(p => p.Name);

            if (row.Columns.Any(c => !tProperties.ContainsKey(c)))
            {
                //4/27/2015: Choosing to ignore this situation for now instead of throwing an exception. It seemed to cause more problems than it solved.
                //var unmatched = string.Join(",", row.Columns.Where(c => !tProperties.ContainsKey(c)));
                //throw new Exception("At least one of the Row's columns doesn't exist as a property on object '" + tType.Name + "'. (" + unmatched + ")");
            }

            foreach (var column in row.Columns.Where(tProperties.ContainsKey))
            {
                var property = tProperties[column];
                var value    = row[column].To(property.PropertyType);

                property.SetValue(tInstance, value);
            }

            return tInstance;
        }

        public static string RowToJson(IRow row)
        {
            var json = new StringBuilder();

            json.Append("{");

            var orderedColumns = row.Columns.OrderBy(c => c);

            foreach (var column in orderedColumns)
            {
                json.Append("\"").Append(column).Append("\"").Append(":").Append("\"").Append(row[column]).Append("\"").Append(",");
            }

            json.Remove(json.Length - 1, 1);

            json.Append("}");

            return json.ToString();

        }
        #endregion

        #region Constructor
        internal BaseRow(){ }
        #endregion

        #region IEnumerable Implementation
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return Columns.Select(column => new KeyValuePair<string, string>(column, this[column])).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region IRow Implementation
        public abstract IEnumerable<string> Values { get; }
        public abstract IEnumerable<string> Columns { get; }

        public abstract string this[string column] { get; set; }

        public abstract void Add(string column, object value);

        public abstract void Remove(string column);
        #endregion

        #region IComparable Implementationt
        public int CompareTo(object obj)
        {            
            var lefts  = Values.ToArray();
            var rights = (obj as IRow)?.Values.ToArray() ?? new string[] {};

            for(var i = 0; i < Math.Min(lefts.Length, rights.Length); i++)
            {
                if (string.Compare(lefts[i], rights[i], StringComparison.Ordinal) != 0)
                {
                    return string.Compare(lefts[i], rights[i], StringComparison.Ordinal);
                }
            }

            return lefts.Length.CompareTo(rights.Length);
        }
        #endregion

        #region Protected Methods
        protected void AssertHasColumn(string column)
        {
            if (!Columns.Contains(column)) throw new ColumnNotFoundException(column);
        }
        #endregion

        #region Equals Overload
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj == this) return true;

            var row = obj as BaseRow;

            if (row == null) return false;

            var rowPairs = row.AsEnumerable();
            var thisPairs = this.AsEnumerable();

            return rowPairs.SequenceEqual(thisPairs);
        }

        public override int GetHashCode()
        {
            var sb = new StringBuilder();

            foreach (var value in Values)
            {
                sb.Append(value);
            }

            return sb.ToString().ToLower().GetHashCode();
        }        
        #endregion

        #region Private Methods
        private static object Value(DataRow dataRow, string name)
        {
            var value = dataRow[name];

            return  value is DBNull ? null : value;
        }

        private static Type Type(DataRow dataRow, string name)
        {
            var column = dataRow.Table.Columns[name];

            return (column.DataType.IsValueType && column.AllowDBNull) ? typeof(Nullable<>).MakeGenericType(column.DataType) : column.DataType;
        }
        #endregion
    }
}