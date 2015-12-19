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
        [Test]
        public void Should_work()
        {
            var expectedCar1 = new Car { Brand = "audi", Name = "a4", Price = 19990.0m };
            var expectedCar2 = new Car { Brand = "audi", Name = "a8", Price = 69990.0m };

            var marketplace = new Marketplace();

            marketplace.Add(expectedCar1);
            marketplace.Add(expectedCar2);
            //Act
            var anotherMarketplace = new Marketplace();
            var result = anotherMarketplace.Search("audi");
            //Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.EqualTo(2));

            expectedCar1.ToExpectedObject().ShouldEqual(result[0]);
            expectedCar2.ToExpectedObject().ShouldEqual(result[1]);
        }
    }

    public class Marketplace
    {
        private CarRepository carRepository;

        public Car[] Search(string brand)
        {
            throw new NotImplementedException();
        }

        public void Add(Car car)
        {
            carRepository.Add(car);
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
