using System;
using System.Diagnostics.CodeAnalysis;

namespace Rucker.Sql
{
    /// <remarks>
    /// It is important these all remain lowercased so they seemlessly translate between .NET and Sql Server
    /// </remarks>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [Flags]
    public enum TSqlTypes
    {
        @int              = 1,
        @bigint           = 1 << 01,
        @smallint         = 1 << 02,
        @float            = 1 << 03,
        @decimal          = 1 << 04,
        @money            = 1 << 05,
        @smallmoney       = 1 << 06,
        @numeric          = 1 << 07,
        @bit              = 1 << 08,
        @char             = 1 << 09,
        @varchar          = 1 << 10,
        @nchar            = 1 << 11,
        @nvarchar         = 1 << 12,
        @text             = 1 << 13,
        @date             = 1 << 14,
        @datetime         = 1 << 15,
        @datetime2        = 1 << 16,
        @smalldatetime    = 1 << 17,
        @uniqueidentifier = 1 << 18,
        @varbinary        = 1 << 19,

        Numbers = @int | bigint | smallint | @float | @decimal | money | smallmoney | numeric
    }

    public static class SqlTypeExtensions
    {
        /// <remarks>
        /// See here for this list: https://msdn.microsoft.com/en-us/library/cc716729(v=vs.110).aspx
        /// </remarks>
        public static DotNetTypes ToDotNetTypes(this TSqlTypes sqlTypes)
        {
            DotNetTypes output = 0;

            if ((sqlTypes & TSqlTypes.@int) == sqlTypes)
            {
                output = output | DotNetTypes.Int;
            }

            if ((sqlTypes & TSqlTypes.@bigint) == sqlTypes)
            {
                output = output | DotNetTypes.Long;
            }

            if ((sqlTypes & TSqlTypes.@smallint) == sqlTypes)
            {
                output = output | DotNetTypes.Short;
            }

            if ((sqlTypes & TSqlTypes.@float) == sqlTypes)
            {
                output = output | DotNetTypes.Double;
            }

            if ((sqlTypes & (TSqlTypes.@decimal | TSqlTypes.@money | TSqlTypes.@smallmoney | TSqlTypes.@numeric)) == sqlTypes)
            {
                output = output | DotNetTypes.Decimal;
            }

            if ((sqlTypes & TSqlTypes.@bit) == sqlTypes)
            {
                output = output | DotNetTypes.Boolean;
            }

            if ((sqlTypes & (TSqlTypes.@char | TSqlTypes.@varchar | TSqlTypes.@nchar | TSqlTypes.@nvarchar | TSqlTypes.@text)) == sqlTypes)
            {
                output = output | DotNetTypes.String;
            }

            if ((sqlTypes & (TSqlTypes.@date | TSqlTypes.@datetime | TSqlTypes.@datetime2 | TSqlTypes.@smalldatetime) ) == sqlTypes)
            {
                output = output | DotNetTypes.DateTime;
            }

            return output;
        }
    }
}