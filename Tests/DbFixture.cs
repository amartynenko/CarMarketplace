using System.Data.SqlClient;
using Dapper;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public abstract class DbFixture
    {
        [SetUp]
        public void SetUp()
        {
            Execute("TRUNCATE TABLE Car");
            Execute("TRUNCATE TABLE Purchase");

            DoSetUp();
        }

        protected const string TestDbConnectionString = "Data Source=localhost;Initial Catalog=CarMarketPlace;Integrated Security=True";
        protected abstract void DoSetUp();

        private SqlConnection OpenConnection()
        {
            var connection = new SqlConnection(TestDbConnectionString);
            connection.Open();

            return connection;
        }

        private void Execute(string sql, dynamic param = null)
        {
            using (var conn = OpenConnection())
                SqlMapper.Execute(conn, sql, param);
        }
    }
}