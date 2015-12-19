using System;
using System.Data;
using System.Data.SqlClient;
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

        [Test]
        public void Should_build_sales_report()
        {
            var marketplace = new Marketplace(TestDbConnectionString);

            marketplace.Purchase("audi", "a4", Guid.NewGuid().ToString());
            marketplace.Purchase("audi", "a4", Guid.NewGuid().ToString());
            marketplace.Purchase("bmw", "520d", Guid.NewGuid().ToString());
            marketplace.Purchase("bmw", "520d", Guid.NewGuid().ToString());
            marketplace.Purchase("bmw", "520d", Guid.NewGuid().ToString());
            marketplace.Purchase("bmw", "520d", Guid.NewGuid().ToString());
            //Act
            var result = marketplace.GetSalesReport();
            //Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.EqualTo(2));

            var expectedAudiSales = new CarSales { Brand = "audi", Name = "a4", Count = 2, TotalPrice = 2 * audiA4.Price };
            var expectedBmwSales = new CarSales { Brand = "bmw", Name = "520d", Count = 4, TotalPrice = 4 * bmw520d.Price };

            expectedBmwSales.ToExpectedObject().ShouldMatch(result[0]);
            expectedAudiSales.ToExpectedObject().ShouldMatch(result[1]);
        }
    }
}
