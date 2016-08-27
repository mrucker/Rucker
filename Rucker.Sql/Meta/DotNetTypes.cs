using System;

namespace Rucker.Sql
{
    [Flags]
    public enum DotNetTypes
    {
        //base
        Unknown  = 0,
        Int      = 1,
        Long     = 1 << 1, //2
        Short    = 1 << 2, //4
        Double   = 1 << 3, //8
        Decimal  = 1 << 4, //etc...
        Boolean  = 1 << 5,
        String   = 1 << 6,
        DateTime = 1 << 7,
        
        //rollups
        Numeric  = Int | Long | Short | Double | Decimal,
        Any      = Int | Long | Short | Double | Decimal | Boolean | String | DateTime
    }

    public static class DotNetTypeExtensions
    {
        /// <remarks>
        /// See here for this list: https://msdn.microsoft.com/en-us/library/cc716729(v=vs.110).aspx
        /// </remarks>
        public static TSqlTypes ToSqlTypes(this DotNetTypes dotNetTypes)
        {
            TSqlTypes output = 0;

            if ((dotNetTypes & DotNetTypes.Int) == DotNetTypes.Int)
            {
                output = output | TSqlTypes.@int;
            }

            if ((dotNetTypes & DotNetTypes.Long) == DotNetTypes.Long)
            {
                output = output | TSqlTypes.@bigint;
            }

            if ((dotNetTypes & DotNetTypes.Short) == DotNetTypes.Short)
            {
                output = output | TSqlTypes.@smallint;
            }

            if ((dotNetTypes & DotNetTypes.Double) == DotNetTypes.Double)
            {
                output = output | TSqlTypes.@float;
            }

            if ((dotNetTypes & DotNetTypes.Decimal) == DotNetTypes.Decimal)
            {
                output = output | TSqlTypes.@decimal;
                output = output | TSqlTypes.@money;
                output = output | TSqlTypes.@smallmoney;
                output = output | TSqlTypes.@numeric;
            }

            if ((dotNetTypes & DotNetTypes.Boolean) == DotNetTypes.Boolean)
            {
                output = output | TSqlTypes.@bit;
            }

            if ((dotNetTypes & DotNetTypes.String) == DotNetTypes.String)
            {
                output = output | TSqlTypes.@char;
                output = output | TSqlTypes.@varchar;
                output = output | TSqlTypes.@nchar;
                output = output | TSqlTypes.@nvarchar;
                output = output | TSqlTypes.@text;
            }

            if ((dotNetTypes & DotNetTypes.DateTime) == DotNetTypes.DateTime)
            {
                output = output | TSqlTypes.@date;
                output = output | TSqlTypes.@datetime;
                output = output | TSqlTypes.@datetime2;
                output = output | TSqlTypes.@smalldatetime;
            }

            return output;
        }      
    }

}