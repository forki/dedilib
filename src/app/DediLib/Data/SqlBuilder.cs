using System;
using System.Linq;
using System.Text;

namespace DediLib.Data
{
    public class SqlBuilder
    {
        public string Select(string tableName, params string[] columnNames)
        {
            if (columnNames == null) throw new ArgumentNullException("columnNames");
            if (tableName == null) throw new ArgumentNullException("tableName");

            var sql = new StringBuilder("SELECT ");
            if (!columnNames.Any())
            {
                sql.Append("*");
            }
            else
            {
                var first = true;
                foreach (var columnName in columnNames)
                {
                    if (!first) sql.Append(",");
                    first = false;

                    sql.Append(columnName);
                }
            }
            sql.Append(" FROM " + tableName);

            return sql.ToString();
        }

        public string Update(string tableName, params string[] columnNames)
        {
            if (tableName == null) throw new ArgumentNullException("tableName");
            if (columnNames == null) throw new ArgumentNullException("columnNames");
            if (!columnNames.Any())
                throw new ArgumentException("Missing column names");

            var sql = new StringBuilder("UPDATE " + tableName + " SET ");

            var first = true;
            foreach (var column in columnNames.Distinct())
            {
                if (!first) sql.Append(",");
                first = false;

                sql.Append(column + "=@" + column);
            }

            return sql.ToString();
        }
    }
}
