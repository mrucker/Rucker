using System;
using System.Collections.Generic;

namespace Rucker.Core
{
    public class ValueRow: BaseRow
    {
        public object Value { get; set; }

        public ValueRow(object value)
        {
            Value = value;
        }

        public override IEnumerable<string> Values
        {
            get { yield return Value.ToString(); }
        }

        public override IEnumerable<string> Columns
        {
            get { yield return "Value"; }
        }

        public override string this[string column]
        {
            get
            {
                if (column != "Value") throw new ColumnNotFoundException(column);
                
                return Value.ToString();
            }
            set
            {
                if (column != "Value") throw new ColumnNotFoundException(column);
                
                Value = value;
            }
        }

        public override void Add(string column, object value)
        {
            throw new Exception("No columns can be added to a value row");
        }

        public override void Remove(string column)
        {
            throw new Exception("No columns can be removed from a value row");
        }
    }
}