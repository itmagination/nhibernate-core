using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;

namespace NHibernate.AdoNet
{
    public class SqlCommandToStringConverter
    {
        const string valuesText = " VALUES ";
        public static string Convert(System.Data.SqlClient.SqlCommand command, bool isFirstCommand)
        {
            if (command.CommandText.StartsWith("DELETE", StringComparison.OrdinalIgnoreCase)
                || command.CommandText.StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase))
            {
                return BuildSpExecCommand(command);
            }

            return BuildCommandQueryForInsert(command, isFirstCommand);
        }

        private static string BuildSpExecCommand(System.Data.SqlClient.SqlCommand command)
        {
            var queryStringBuilder = new StringBuilder("EXECUTE sp_executesql N'" + command.CommandText + "',N'");
            for (var i = 0; i < command.Parameters.Count; i++)
            {
                var p = command.Parameters[i];
                queryStringBuilder.Append(p.ParameterName + " " + p.SqlDbType);
                AppendLengthForVarLengthFields(p, queryStringBuilder);
                if (i < command.Parameters.Count - 1)
                    queryStringBuilder.Append(",");
            }

            queryStringBuilder.Append("',");
            for (var i = 0; i < command.Parameters.Count; i++)
            {
                var p = command.Parameters[i];
                queryStringBuilder.Append(p.ParameterName + "=" + FormatSqlParameterValue.Format(p));
                if (i < command.Parameters.Count - 1)
                    queryStringBuilder.Append(",");
            }

            return queryStringBuilder.ToString();
        }

        private static void AppendLengthForVarLengthFields(SqlParameter p, StringBuilder query)
        {
            switch (p.SqlDbType)
            {
                case SqlDbType.Char:
                case SqlDbType.NChar:
                case SqlDbType.Binary:
                    query.Append("(" + p.Size + ")");
                    return;

                case SqlDbType.NVarChar:
                case SqlDbType.VarChar:
                case SqlDbType.VarBinary:
                    query.Append("(" + (p.Size > 4000 ? "MAX" : "4000") + ")");
                    return;
                case SqlDbType.Decimal:
                    query.Append("(" + p.Precision + "," + p.Scale + ")");
                    return;
            }
        }

        private static string BuildCommandQueryForInsert(System.Data.SqlClient.SqlCommand command, bool isFirstCommand)
        {
            string query = isFirstCommand ? command.CommandText : GetNthQueryCommandText(command);
	        query = ReplaceParameters(query, command.Parameters);
            return query;
        }

	    private static string ReplaceParameters(string exampleText, SqlParameterCollection commandParameters)
	    {
		    StringBuilder regexBuilder = new StringBuilder("(");
		    IDictionary<string, string> parameterToFormattedValue = new Dictionary<string, string>();
		    for (var i = commandParameters.Count - 1; i >= 0; i--)
		    {
			    var p = commandParameters[i];
			    regexBuilder.Append(p.ParameterName);
			    regexBuilder.Append("|");
			    parameterToFormattedValue.Add(p.ParameterName, FormatSqlParameterValue.Format(p));
		    }
		    regexBuilder.Remove(regexBuilder.Length - 1, 1);
		    regexBuilder.Append(")");
		    return Regex.Replace(exampleText, regexBuilder.ToString(), match => parameterToFormattedValue[match.Value]);
	    }

	    private static string GetNthQueryCommandText(System.Data.SqlClient.SqlCommand command)
        {
            var valuesIndex = command.CommandText.IndexOf(valuesText, StringComparison.OrdinalIgnoreCase);
            return "," + command.CommandText.Substring(valuesIndex + valuesText.Length);
        }
    }
}
