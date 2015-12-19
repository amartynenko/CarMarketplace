using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;

namespace Tests
{
    public abstract class Repository
    {
        private readonly string connectionString;

        public Repository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        protected IEnumerable<TResult> Query<TResult>(string sql, dynamic parameters = null)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                return SqlMapper.Query<TResult>(connection, sql, parameters);
            }
        }

        private SqlConnection OpenConnection()
        {
            var connection = new SqlConnection(connectionString);
            connection.Open();

            return connection;
        }

        protected void Execute(string sql, dynamic param = null)
        {
            using (var conn = OpenConnection())
                SqlMapper.Execute(conn, sql, param);
        }
    }
}