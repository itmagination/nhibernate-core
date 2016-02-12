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

                case DbType.SByte:
                case DbType.Byte:
                case DbType.Currency:
                case DbType.Decimal:
                case DbType.Double:
                case DbType.VarNumeric:
                case DbType.Single:
                case DbType.Int16:
                case DbType.Int32:
                case DbType.Int64:
                case DbType.UInt16:
                case DbType.UInt32:
                case DbType.UInt64:
                    if (p.Value is Enum)
                        return ((int)p.Value).ToString();
                    return FormatNumeric(p.Value);

                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                    return "'" + FormatText(p.Value) + "'";

                case DbType.String:
                case DbType.StringFixedLength:
                case DbType.Xml:
                    return "N'" + FormatText(p.Value) + "'";

                case DbType.DateTime:
                case DbType.DateTime2:
                    return "'"+FormatDateTime((DateTime)p.Value) + "'";

                case DbType.DateTimeOffset:
                    return "'" + FormatDateTimeOffset((DateTimeOffset)p.Value) + "'";
                
                case DbType.Date:
                case DbType.Time:
                
                case DbType.Guid:
                case DbType.Object:
//                    return "'" + p.Value + "'";              
                    if(p.Value is Enum)
                        return ((int)p.Value).ToString();
                    return "'" + p.Value + "'";
                default:
                    throw new NotSupportedException(string.Format("Type:{0} was not expected",p.DbType));

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