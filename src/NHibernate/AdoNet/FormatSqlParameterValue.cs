using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace NHibernate.AdoNet
{
    internal class FormatSqlParameterValue
    {
        private const string DateTimeWithMilisecondsFormat = "yyyy-MM-dd HH:mm:ss.fff";
        private const string DateTimeOffsetWithMilisecondsFormat = "yyyy-MM-dd HH:mm:ss.fffffff zzz";

        internal static string Format(SqlParameter p)
        {
            if (IsNull(p))
                return "NULL";
            switch (p.DbType)
            {
                case DbType.Binary:
                    return FormatBinary((byte[]) p.Value);

                case DbType.Boolean:
                    return ((bool) p.Value) ? "1" : "0";

                case DbType.Currency:
                case DbType.Decimal:
                case DbType.Double:
                case DbType.VarNumeric:
                case DbType.Single:
                    return FormatNumeric(p.Value);

                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.String:
                case DbType.StringFixedLength:
                case DbType.Xml:
                    return "'" + FormatText(p.Value) + "'";

                case DbType.DateTime:
                case DbType.DateTime2:
                    return "'"+FormatDateTime((DateTime)p.Value) + "'";

                case DbType.DateTimeOffset:
                    return "'" + FormatDateTimeOffset((DateTimeOffset)p.Value) + "'";
                //case DbType.Time:
                default:

                    if(p.Value is Enum)
                        return ((int)p.Value).ToString();
                    return "'" + p.Value + "'";
            }
        }

        internal static string FormatText(object value)
        {
            if (value is char)
                return value.ToString();
            return ((string) value).Replace("'", "''");
        }

        internal static string FormatNumeric(object value)
        {
            return value.ToString().Replace(",", ".");
        }

        internal static string FormatBinary(byte[] value)
        {
            return "0x" + BitConverter.ToString(value).Replace("-", string.Empty);
        }

        internal static string FormatDateTime(DateTime dateTime)
        {
            return dateTime.ToString(DateTimeWithMilisecondsFormat, CultureInfo.InvariantCulture);
        }

        internal static string FormatDateTimeOffset(DateTimeOffset dateTime)
        {
            return dateTime.ToString(DateTimeOffsetWithMilisecondsFormat, CultureInfo.InvariantCulture);
        }

        internal static bool IsNull(SqlParameter p)
        {
            return p.Value == DBNull.Value;
        }
    }
}