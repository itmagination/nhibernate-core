using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace NHibernate.AdoNet
{
    public class SqlCommandToStringConverter
    {
        const string valuesText = " VALUES ";
        public static string Convert(System.Data.SqlClient.SqlCommand command, bool isFirstCommand)
        {
            if (   command.CommandText.StartsWith("DELETE", StringComparison.InvariantCultureIgnoreCase)
                || command.CommandText.StartsWith("UPDATE", StringComparison.InvariantCultureIgnoreCase))
            {
                return BuildSpExecCommand(command);
            }
            return BuildCommandQueryForInsert(command, isFirstCommand);
        }

        private static string BuildSpExecCommand(System.Data.SqlClient.SqlCommand command)
        {
            StringBuilder query = new StringBuilder("EXECUTE sp_executesql N'" + command.CommandText + "',N'");
            for (var i = 0; i < command.Parameters.Count; i++)
            {
                var p = command.Parameters[i];
                query.Append(p.ParameterName + " " + p.SqlDbType);
                AppendLengthForVarLengthFields(p, query);
                if (i < command.Parameters.Count - 1)
                    query.Append(",");
            }
            query.Append("',");
            for (var i = 0; i < command.Parameters.Count; i++)
            {
                var p = command.Parameters[i];
                query.Append(p.ParameterName + "=" + FormatSqlParameterValue.Format(p));
                if (i < command.Parameters.Count - 1)
                    query.Append(",");
            }
            return query.ToString();
        }

        private static void AppendLengthForVarLengthFields(SqlParameter p, StringBuilder query)
        {
            if (p.SqlDbType == SqlDbType.NVarChar
                || p.SqlDbType == SqlDbType.VarBinary
                || p.SqlDbType == SqlDbType.VarChar
                )
                query.Append("(" + (p.Size > 4000 ? "MAX" : "4000") + ")");
        }

        private static string BuildCommandQueryForInsert(System.Data.SqlClient.SqlCommand command, bool isFirstCommand)
        {
            StringBuilder query;
            query = isFirstCommand ? new StringBuilder(command.CommandText) : GetNthQueryCommandText(command);
            for (var i = command.Parameters.Count - 1; i >= 0; i--)
            {
                var p = command.Parameters[i];
                query.Replace(p.ParameterName, FormatSqlParameterValue.Format(p));
            }
            return query.ToString();
        }

        private static StringBuilder GetNthQueryCommandText(System.Data.SqlClient.SqlCommand command)
        {
            var valuesIndex = command.CommandText.IndexOf(valuesText,StringComparison.InvariantCultureIgnoreCase);
            var query = new StringBuilder("," + command.CommandText.Substring(valuesIndex + valuesText.Length));
            return query;
        }
    }
}
