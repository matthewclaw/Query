using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Query
{
    public enum DbType
    {
        MSSQL,
        MySql
    }
    public static class QueryBase
    {
        public static IEnumerable<T> ExecuteReaderLazy<T>(object parameters, string MySqlQuery, bool mapToObject, MySqlConnection con)
        {
            return ExecuteReaderLazy<T>(parameters, MySqlQuery, mapToObject, con, DbType.MySql);
        }
        public static IEnumerable<T> ExecuteReaderLazy<T>(object parameters, string SqlQuery, bool mapToObject, SqlConnection con)
        {
            return ExecuteReaderLazy<T>(parameters, SqlQuery, mapToObject, con, DbType.MSSQL);
        }
        private static IEnumerable<T> ExecuteReaderLazy<T>(object parameters, string QueryString, bool mapToObject, DbConnection con, DbType dbType)
        {
            using (var command = con.CreateCommand())
            {
                command.CommandText = QueryString;
                if (parameters != null)
                {
                    switch (dbType)
                    {
                        case DbType.MSSQL:
                            PassInParameters(command, parameters, () => new SqlParameter());
                            break;
                        case DbType.MySql:
                            PassInParameters(command, parameters, () => new MySqlParameter());
                            break;
                        default:
                            break;
                    }
                }
                using (var reader = command.ExecuteReader())
                {
                    if (mapToObject)
                    {
                        var ordinals = GetOrdinals<T>(reader);
                        var CreateNewInstance = BuildActivator<T>();
                        while (reader.Read())
                        {
                            yield return CopyFields<T>(ordinals, reader, CreateNewInstance());
                        }
                    }
                    else
                    {
                        while (reader.Read())
                        {
                            yield return (T)reader.GetValue(0);
                        }
                    }
                }
            }
        }
        public static DataTable ReaderIntoDataTable(object parameters, string SqlQuery, MySqlConnection con)
        {
            return ReaderIntoDataTable(parameters, SqlQuery, con, DbType.MySql);
        }
        public static DataTable ReaderIntoDataTable(object parameters, string SqlQuery, SqlConnection con)
        {
            return ReaderIntoDataTable(parameters, SqlQuery, con, DbType.MSSQL);
        }
        private static DataTable ReaderIntoDataTable(object parameters, string QueryString, DbConnection con, DbType dbType)
        {
            using (var command = con.CreateCommand())
            {
                command.CommandText = QueryString;
                if (parameters != null)
                {
                    switch (dbType)
                    {
                        case DbType.MSSQL:
                            PassInParameters(command, parameters, () => new SqlParameter());
                            break;
                        case DbType.MySql:
                            PassInParameters(command, parameters, () => new MySqlParameter());
                            break;
                        default:
                            break;
                    }
                }
                using (var reader = command.ExecuteReader())
                {
                    var dataTable = new DataTable();
                    dataTable.Load(reader);
                    return dataTable;
                }
            }
        }
        public static DbDataReader ExecuteReader(object parameters, string SqlQuery, MySqlConnection con)
        {
            return ExecuteReader(parameters, SqlQuery, con, DbType.MySql);

        }
        public static DbDataReader ExecuteReader(object parameters, string SqlQuery, SqlConnection con)
        {
            return ExecuteReader(parameters, SqlQuery, con, DbType.MSSQL);
        }
        private static DbDataReader ExecuteReader(object parameters, string SqlQuery, DbConnection con, DbType dbType)
        {
            using (var command = con.CreateCommand())
            {
                command.CommandText = SqlQuery;
                switch (dbType)
                {
                    case DbType.MSSQL:
                        PassInParameters(command, parameters, () => new SqlParameter());
                        break;
                    case DbType.MySql:
                        PassInParameters(command, parameters, () => new MySqlParameter());
                        break;
                    default:
                        break;
                }
                return command.ExecuteReader();
            }
        }
        private static T ExecuteScalar<T>(object parameters, string SqlQuery, DbConnection con, DbType dbType, out bool HasResult)
        {
            HasResult = true;
            using (var command = con.CreateCommand())
            {
                command.CommandText = SqlQuery;
                switch (dbType)
                {
                    case DbType.MSSQL:
                        PassInParameters(command, parameters, () => new SqlParameter());
                        break;
                    case DbType.MySql:
                        PassInParameters(command, parameters, () => new MySqlParameter());
                        break;
                    default:
                        break;
                }
                var result = command.ExecuteScalar();
                if (result == null)
                {
                    HasResult = false;
                    return default(T);
                }
                else
                {
                    return (T)result;
                }
            }
        }
        public static T ExecuteScalar<T>(object parameters, string SqlQuery, MySqlConnection con, out bool HasResult)
        {
            return ExecuteScalar<T>(parameters, SqlQuery, con, DbType.MySql, out HasResult);
        }
        public static T ExecuteScalar<T>(object parameters, string SqlQuery, SqlConnection con, out bool HasResult)
        {
            return ExecuteScalar<T>(parameters, SqlQuery, con, DbType.MSSQL, out HasResult);
        }
        public static int ExecuteNonQuery(object parameters, string SqlQuery, MySqlConnection con, CommandType cmdType = CommandType.Text)
        {
            return ExecuteNonQuery(parameters, SqlQuery, con, DbType.MySql, cmdType);
        }
        public static int ExecuteNonQuery(object parameters, string SqlQuery, SqlConnection con, CommandType cmdType = CommandType.Text)
        {
            return ExecuteNonQuery(parameters, SqlQuery, con, DbType.MSSQL, cmdType);
        }
        private static int ExecuteNonQuery(object parameters, string SqlQuery, DbConnection con, DbType dbType, CommandType cmdType = CommandType.Text)
        {
            using (var command = con.CreateCommand())
            {
                command.CommandText = SqlQuery;
                command.CommandType = cmdType;
                switch (dbType)
                {
                    case DbType.MSSQL:
                        PassInParameters(command, parameters, () => new SqlParameter());
                        break;
                    case DbType.MySql:
                        PassInParameters(command, parameters, () => new MySqlParameter());
                        break;
                    default:
                        break;
                }
                return command.ExecuteNonQuery();
            }
        }
        private sealed class OrdinalPair
        {
            public MemberInfo Info;
            public int Ordinal;
        }

        private static OrdinalPair[] GetOrdinals<T>(DbDataReader reader)
        {
            var properties = typeof(T)
                .GetProperties(BindingFlags.Public)
                .Where(x => x.CanWrite && x.CanRead && reader.HasColumn(x.Name))
                .Select(propertyInfo => new OrdinalPair { Info = propertyInfo, Ordinal = reader.GetOrdinal(propertyInfo.Name) });
            var fields = typeof(T)
                .GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                .Where(fieldInfo => reader.HasColumn(fieldInfo.Name))
                .Select(fieldInfo => new OrdinalPair { Info = fieldInfo, Ordinal = reader.GetOrdinal(fieldInfo.Name) });
            return properties.Concat(fields).ToArray();
        }

        private static T CopyFields<T>(OrdinalPair[] ordinals, DbDataReader reader, T res)
        {
            foreach (var member in ordinals)
            {
                if (!reader.IsDBNull(member.Ordinal))
                {
                    if (member.Info is PropertyInfo)
                    {
                        var propertyInfo = (PropertyInfo)member.Info;
                        var val = reader.GetValue(member.Ordinal);
                        if (propertyInfo.PropertyType == typeof(char))
                        {
                            propertyInfo.SetValue(res, GetCharVal(val));
                        }
                        else
                        {
                            propertyInfo.SetValue(res, val);
                        }

                    }
                    else if (member.Info is FieldInfo)
                    {
                        var fieldInfo = (FieldInfo)member.Info;
                        var val = reader.GetValue(member.Ordinal);
                        if (fieldInfo.FieldType == typeof(char))
                        {
                            fieldInfo.SetValue(res, GetCharVal(val));
                        }
                        else
                        {
                            fieldInfo.SetValue(res, val);
                        }
                    }
                }
            }
            return res;
        }

        private static char GetCharVal(object o)
        {
            var s = (string)o;
            if (s.Length > 0)
            {
                return s.ToString()[0];
            }
            else
            {
                return '\0'; // interpret an empty string as null
            }
        }

        public static Func<T> BuildActivator<T>() // Builds the equivalent of: Activator.CreateInstance<T>()
        {
            var ctor = typeof(T).GetConstructors().First();
            var newExpr = Expression.New(ctor);
            var lambda = Expression.Lambda<Func<T>>(newExpr);
            return lambda.Compile();
        }

        private static void PassInParameters(DbCommand command, object parameters, Func<DbParameter> GetParameter)
        {
            var returnType = GetParameter().GetType().Name;
            switch (returnType)
            {
                case "SqlParameter":
                    if (parameters.GetType() != typeof(SqlParameterCollection))
                    {
                        foreach (var paramInfo in parameters.GetType().GetProperties())
                        {
                            if (paramInfo.Name != "Type")
                            {
                                var param = GetParameter();
                                param.Value = paramInfo.GetValue(parameters);
                                param.ParameterName = paramInfo.Name;
                                command.Parameters.Add(param);
                            }
                        }
                    }
                    else
                    {
                        foreach (SqlParameter par in (SqlParameterCollection)parameters)
                        {
                            var param = GetParameter();
                            param.Value = par.Value;
                            param.ParameterName = par.ParameterName;
                            command.Parameters.Add(param);

                        }
                    }
                    break;
                case "MySqlParameter":
                    if (parameters.GetType() != typeof(MySqlParameterCollection))
                    {
                        foreach (var paramInfo in parameters.GetType().GetProperties())
                        {
                            if (paramInfo.Name != "Type")
                            {
                                var param = GetParameter();
                                param.Value = paramInfo.GetValue(parameters);
                                param.ParameterName = paramInfo.Name;
                                command.Parameters.Add(param);
                            }
                        }
                    }
                    else
                    {
                        foreach (MySqlParameter par in (MySqlParameterCollection)parameters)
                        {
                            var param = GetParameter();
                            param.Value = par.Value;
                            param.ParameterName = par.ParameterName;
                            command.Parameters.Add(param);

                        }
                    }
                    break;
            }

        }

        public static bool HasColumn(this IDataRecord dr, string columnName)
        {
            for (int i = 0; i < dr.FieldCount; i++)
            {
                if (dr.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
