using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using ExpectedObjects;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Tests
{
    [TestFixture]
    public class Tests
    {
        private Car car1;
        private Car car2;
        private Car car3;
        private const string TestDbConnectionString = "Data Source=localhost;Initial Catalog=CarMarketPlace;Integrated Security=True";

        [SetUp]
        public void SetUp()
        {
            Execute("TRUNCATE TABLE Car");

            car1 = new Car { Brand = "audi", Name = "a4", Price = 19990.0m };
            car2 = new Car { Brand = "audi", Name = "a8", Price = 69990.0m };
            car3 = new Car { Brand = "bmw", Name = "520d", Price = 49990.0m };

            var marketplace = new Marketplace(TestDbConnectionString);

            marketplace.Add(car1);
            marketplace.Add(car2);
            marketplace.Add(car3);
        }

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

        [Test]
        public void Should_search_by_brand()
        {
            //Act
            var marketplace = new Marketplace(TestDbConnectionString);
            var result = marketplace.Search("audi");
            //Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.EqualTo(2));

            car1.ToExpectedObject().ShouldEqual(result[0]);
            car2.ToExpectedObject().ShouldEqual(result[1]);
        }

        [Test]
        public void Should_list_all_cars()
        {
            //Act
            var marketplace = new Marketplace(TestDbConnectionString);
            var result = marketplace.ListCars();
            //Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.EqualTo(3));

            car1.ToExpectedObject().ShouldEqual(result[0]);
            car2.ToExpectedObject().ShouldEqual(result[1]);
            car3.ToExpectedObject().ShouldEqual(result[2]);
        }
    }

    public class Marketplace
    {
        private readonly CarRepository carRepository;

        public Marketplace(string connectionString)
        {
            carRepository = new CarRepository(connectionString);
        }

        public Car[] Search(string brand)
        {
            return carRepository.Search(brand)
                .ToArray();
        }

        public void Add(Car car)
        {
            carRepository.Add(car);
        }

        public Car[] ListCars()
        {
            return carRepository.ListAll().ToArray();
        }
    }

    public class CarRepository
    {
        private readonly string connectionString;

        public CarRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public void Add(Car car)
        {
            Execute(@"INSERT INTO [dbo].[Car] (
                    [Name], 
                    [Brand],
                    [Price]
                ) 
                VALUES (
                    @Name, 
                    @Brand, 
                    @Price
                );", car);
        }

        public IEnumerable<Car> Search(string brand)
        {
            return Query<Car>(@"SELECT 
                                    [Name], 
                                    [Brand], 
                                    [Price] 
                                FROM [dbo].[Car]
                                WHERE [Brand] = @Brand", new { Brand = brand });
        }

        public IEnumerable<Car> ListAll()
        {
            return Query<Car>(@"SELECT 
                                    [Name], 
                                    [Brand], 
                                    [Price] 
                                FROM [dbo].[Car]");
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

        private void Execute(string sql, dynamic param = null)
        {
            using (var conn = OpenConnection())
                SqlMapper.Execute(conn, sql, param);
        }
    }

    public class Car
    {
        public string Name { get; set; }
        public string Brand { get; set; }
        public decimal Price { get; set; }
    }
}
