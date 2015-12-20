using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;

namespace MarketPlaceService
{
    public abstract class Repository
    {
        private readonly string connectionString;

        protected Repository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        protected async Task<IEnumerable<TResult>> QueryAsync<TResult>(string sql, dynamic parameters = null)
        {
            using (var connection = OpenConnection())
            {
                return await SqlMapper.QueryAsync<TResult>(connection, sql, parameters);
            }
        }

        private SqlConnection OpenConnection()
        {
            var connection = new SqlConnection(connectionString);
            connection.Open();

            return connection;
        }

        protected async Task<int> ExecuteAsync(string sql, dynamic param = null)
        {
            using (var conn = OpenConnection())
            {
                return await SqlMapper.ExecuteAsync(conn, sql, param);
            }
        }
    }
}