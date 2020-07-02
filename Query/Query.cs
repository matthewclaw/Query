using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Query
{
    public class Query
    {
        private object Parameters = null;
        private bool MapToObject = true;
        private string QueryString;
        private DbConnection Con = null;
        private MySqlConnection MySqlCon
        {
            get
            {
                if (type == DbType.MySql)
                {
                    return Con as MySqlConnection;
                }
                else
                {
                    return null;
                }
            }
        }
        private SqlConnection SqlCon
        {
            get
            {
                if (type == DbType.MSSQL)
                {
                    return Con as SqlConnection;
                }
                else
                {
                    return null;
                }
            }
        }
        private bool lazy = false;
        private object DefaultValue;
        private DbType type;
        private Query(string QueryString)
        {
            this.QueryString = QueryString;
        }
        public Query() { }
        public static Query Create(string QueryString)
        {
            return new Query(QueryString);
        }

        public Query WithParameters(object parameters)
        {
            Parameters = parameters;
            return this;
        }

        public Query WithConnection(MySqlConnection con)
        {
            Con = con;
            this.type = DbType.MySql;
            return this;
        }
        public Query WithConnection(SqlConnection con)
        {
            Con = con;
            this.type = DbType.MSSQL;
            return this;
        }

        public Query IsPrimitive()
        {
            MapToObject = false;
            return this;
        }

        public Query Lazy()
        {
            lazy = true;
            return this;
        }
        public Query Default(object Default)
        {
            this.DefaultValue = Default;
            return this;
        }
        public IEnumerable<T> ExecuteReader<T>()
        {
            switch (type)
            {
                case DbType.MSSQL:
                    if (lazy)
                    {
                        return QueryBase.ExecuteReaderLazy<T>(Parameters, QueryString, MapToObject, SqlCon);
                    }
                    else
                    {
                        return QueryBase.ExecuteReaderLazy<T>(Parameters, QueryString, MapToObject, SqlCon).ToList();
                    }
                case DbType.MySql:
                    if (lazy)
                    {
                        return QueryBase.ExecuteReaderLazy<T>(Parameters, QueryString, MapToObject, MySqlCon);
                    }
                    else
                    {
                        return QueryBase.ExecuteReaderLazy<T>(Parameters, QueryString, MapToObject, MySqlCon).ToList();
                    }
                default:
                    return null;
            }

        }
        public T ExecuteScalar<T>()
        {
            try
            {
                object result = null;
                bool hasResult = false;
                switch (type)
                {
                    case DbType.MSSQL:
                        result = QueryBase.ExecuteScalar<T>(Parameters, QueryString, SqlCon, out hasResult);
                        break;
                    case DbType.MySql:
                        result = QueryBase.ExecuteScalar<T>(Parameters, QueryString, MySqlCon, out hasResult);
                        break;
                    default:
                        return (T)DefaultValue;
                }
                if (!hasResult)
                {
                    return (T)DefaultValue;
                }
                else
                {
                    return (T)result;
                }
            }
            catch
            {
                return default(T);
            }
        }
        public DataTable ReaderIntoDataTable()
        {
            switch (type)
            {
                case DbType.MSSQL:
                    return QueryBase.ReaderIntoDataTable(Parameters, QueryString, SqlCon);
                case DbType.MySql:
                    return QueryBase.ReaderIntoDataTable(Parameters, QueryString, MySqlCon);
                default:
                    return null;
            }
        }

        public DbDataReader ExecuteReader()
        {
            switch (type)
            {
                case DbType.MSSQL:
                    return QueryBase.ExecuteReader(Parameters, QueryString, SqlCon);
                case DbType.MySql:
                    return QueryBase.ExecuteReader(Parameters, QueryString, MySqlCon);
                default:
                    return null;
            }
        }

        public int ExecuteNonQuery()
        {
            switch (type)
            {
                case DbType.MSSQL:
                    return QueryBase.ExecuteNonQuery(Parameters, QueryString, SqlCon);
                case DbType.MySql:
                    return QueryBase.ExecuteNonQuery(Parameters, QueryString, MySqlCon);
                default:
                    return 0;
            }
        }
        public override string ToString()
        {
            return "Query: " + QueryString + "\n" + string.Join("\n", Parameters.GetType().GetProperties().Select(p => p.Name + " = " + p.GetValue(Parameters)));
        }
    }
}
