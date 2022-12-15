using System;
using System.Diagnostics.CodeAnalysis;

namespace Data.Sql
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class MetaColumn
    {
        public bool Nullable { get; set; }
        public string Name { get; set; } 
        public TSqlTypes TSqlType { get; set; }
        public DotNetTypes DotNetType => TSqlType.ToDotNetTypes();

        public bool IsType(DotNetTypes dotNetTypes)
        {
            return (DotNetType & dotNetTypes) == DotNetType;
        }

        public bool IsType(TSqlTypes tsqlTypes)
        {
            return (TSqlType & tsqlTypes) == TSqlType;
        }

        public Type Type()
        {
            if ((DotNetType & DotNetTypes.Int) == DotNetType)
            {
                return typeof(int);
            }

            if ((DotNetType & DotNetTypes.Long) == DotNetType)
            {
                return typeof(long);
            }

            if ((DotNetType & DotNetTypes.Short) == DotNetType)
            {
                return typeof(short);
            }

            if ((DotNetType & DotNetTypes.Double) == DotNetType)
            {
                return typeof(double);
            }

            if ((DotNetType & DotNetTypes.Decimal) == DotNetType)
            {
                return typeof(decimal);
            }

            if ((DotNetType & DotNetTypes.Boolean) == DotNetType)
            {
                return typeof(bool);
            }

            if ((DotNetType & DotNetTypes.String) == DotNetType)
            {
                return typeof(string);
            }

            if ((DotNetType & DotNetTypes.DateTime) == DotNetType)
            {
                return typeof(DateTime);
            }

            throw new Exception("Unknown DotNetType");
        }
    }
}