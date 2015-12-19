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
    public class MarketplaceTests
    {
        private Car audiA4;
        private Car audiA8;
        private Car bmw520d;
        private const string TestDbConnectionString = "Data Source=localhost;Initial Catalog=CarMarketPlace;Integrated Security=True";

        [SetUp]
        public void SetUp()
        {
            Execute("TRUNCATE TABLE Car");
            Execute("TRUNCATE TABLE Purchase");

            audiA4 = new Car { Brand = "audi", Name = "a4", Price = 19990.0m };
            audiA8 = new Car { Brand = "audi", Name = "a8", Price = 69990.0m };
            bmw520d = new Car { Brand = "bmw", Name = "520d", Price = 49990.0m };

            var marketplace = new Marketplace(TestDbConnectionString);

            marketplace.Add(audiA4);
            marketplace.Add(audiA8);
            marketplace.Add(bmw520d);
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

            audiA4.ToExpectedObject().ShouldEqual(result[0]);
            audiA8.ToExpectedObject().ShouldEqual(result[1]);
        }

        [Test]
        public void Should_search_by_brand_and_name()
        {
            //Act
            var marketplace = new Marketplace(TestDbConnectionString);
            var result = marketplace.Search("audi", "a8");
            //Assert
            audiA8.ToExpectedObject().ShouldEqual(result);
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

            audiA4.ToExpectedObject().ShouldEqual(result[0]);
            audiA8.ToExpectedObject().ShouldEqual(result[1]);
            bmw520d.ToExpectedObject().ShouldEqual(result[2]);
        }

        [Test]
        public void Should_buy_a_car()
        {
            var marketplace = new Marketplace(TestDbConnectionString);
            var userId = Guid.NewGuid().ToString();
            //Act
            var result = marketplace.Purchase("audi", "a4", userId);
            //Assert
            Assert.That(result, Is.True);

            var purchaseHistory = marketplace.GetPurchaseHistory(userId);
            Assert.That(purchaseHistory.Length, Is.EqualTo(1));
            Assert.That(purchaseHistory[0].Brand, Is.EqualTo("audi"));
            Assert.That(purchaseHistory[0].Name, Is.EqualTo("a4"));
            Assert.That(purchaseHistory[0].UserId, Is.EqualTo(userId));
            Assert.That(purchaseHistory[0].Price, Is.EqualTo(19990.0m));

        }

        [Test]
        public void Should_not_buy_nonexisting_car()
        {
            var marketplace = new Marketplace(TestDbConnectionString);
            var userId = Guid.NewGuid().ToString();
            //Act
            var result = marketplace.Purchase("lexus", "rx350", userId);
            //Assert
            Assert.That(result, Is.False);

            var purchaseHistory = marketplace.GetPurchaseHistory(userId);
            Assert.That(purchaseHistory.Length, Is.EqualTo(0));
        }
    }

    public class Marketplace
    {
        private readonly CarRepository carRepository;
        private readonly PurchaseRepository purchaseRepository;

        public Marketplace(string connectionString)
        {
            purchaseRepository = new PurchaseRepository(connectionString);
            carRepository = new CarRepository(connectionString);
        }

        public Car[] Search(string brand)
        {
            return carRepository.Search(brand)
                .ToArray();
        }

        public Car Search(string brand, string name)
        {
            return carRepository.Search(brand, name);
        }

        public void Add(Car car)
        {
            carRepository.Add(car);
        }

        public Car[] ListCars()
        {
            return carRepository.ListAll().ToArray();
        }

        public bool Purchase(string brand, string name, string userId)
        {
            var car = carRepository.Search(brand, name);
            if (car == null)
                return false;

            decimal price = car.Price;
            var purchase = new Purchase
            {
                Brand = brand,
                Name = name,
                UserId = userId,
                Time = DateTimeOffset.Now,
                Price = price
            };

            purchaseRepository.Purchase(purchase);

            return true;
        }

        public Purchase[] GetPurchaseHistory(string userId)
        {
            return purchaseRepository.GetPurchaseHistory(userId)
                .ToArray();
        }
    }

    public class CarRepository : Repository
    {
        public CarRepository(string connectionString)
            : base(connectionString)
        { }

        public void Add(Car car)
        {
            var sql = @"INSERT INTO [dbo].[Car] (
                            [Name], 
                            [Brand],
                            [Price]
                        ) 
                        VALUES (
                            @Name, 
                            @Brand, 
                            @Price
                        );";

            Execute(sql, car);
        }

        public IEnumerable<Car> Search(string brand)
        {
            var sql = @"SELECT 
                            [Name], 
                            [Brand], 
                            [Price] 
                        FROM [dbo].[Car]
                        WHERE [Brand] = @Brand";

            return Query<Car>(sql, new { Brand = brand });
        }

        public IEnumerable<Car> ListAll()
        {
            return Query<Car>(@"SELECT 
                                    [Name], 
                                    [Brand], 
                                    [Price] 
                                FROM [dbo].[Car]");
        }

        public Car Search(string brand, string name)
        {
            var sql = @"SELECT 
                            [Name], 
                            [Brand], 
                            [Price] 
                        FROM [dbo].[Car]
                        WHERE [Brand] = @Brand AND [Name] = @Name";

            return Query<Car>(sql, new { Brand = brand, Name = name })
                .FirstOrDefault();
        }
    }

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

    public class PurchaseRepository : Repository
    {
        public PurchaseRepository(string connectionString)
            : base(connectionString)
        { }

        public void Purchase(Purchase purchase)
        {
            Execute(@"INSERT INTO [dbo].[Purchase] (
                    [Name], 
                    [Brand],
                    [Price],
                    [UserId],
                    [Time]
                ) 
                VALUES (
                    @Name, 
                    @Brand, 
                    @Price,
                    @UserId,
                    @Time
                );", purchase);
        }

        public IEnumerable<Purchase> GetPurchaseHistory(string userId)
        {
            var sql = @"SELECT 
                            [Name], 
                            [Brand], 
                            [UserId], 
                            [Time],
                            [Price]
                        FROM [dbo].[Purchase]
                        WHERE [UserId] = @UserId";

            return Query<Purchase>(sql, new { UserId = userId });
        }
    }

    public class Car
    {
        public string Name { get; set; }
        public string Brand { get; set; }
        public decimal Price { get; set; }
    }

    public class Purchase
    {
        public string Name { get; set; }
        public string Brand { get; set; }
        public decimal Price { get; set; }
        public string UserId { get; set; }
        public DateTimeOffset Time { get; set; }
    }
}
