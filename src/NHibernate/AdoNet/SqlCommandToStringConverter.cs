using System.Data.SqlClient;
using System.Text;

namespace NHibernate.AdoNet
{
    public class SqlCommandToStringConverter
    {
        public static string Convert(System.Data.SqlClient.SqlCommand command)
        {
            var query = new StringBuilder(command.CommandText);

            for (int i = command.Parameters.Count - 1; i >= 0; i--)
            {
                SqlParameter p = command.Parameters[i];
                query.Replace(p.ParameterName, FormatSqlParameterValue.Format(p));
            }
            return query.ToString();
        }
    }
}