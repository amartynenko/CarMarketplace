using System;
using System.Threading.Tasks;
using ExpectedObjects;
using MarketPlaceService;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class MarketplaceTests : DbFixture
    {
        private Car audiA4;
        private Car audiA8;
        private Car bmw520d;

        protected override void DoSetUp()
        {
            audiA4 = new Car { Brand = "audi", Name = "a4", Price = 19990.0m };
            audiA8 = new Car { Brand = "audi", Name = "a8", Price = 69990.0m };
            bmw520d = new Car { Brand = "bmw", Name = "520d", Price = 49990.0m };

            var marketplace = new Marketplace(TestDbConnectionString);

            marketplace.AddAsync(audiA4).Wait();
            marketplace.AddAsync(audiA8).Wait();
            marketplace.AddAsync(bmw520d).Wait();
        }

        [Test]
        public async Task Should_search_by_brand()
        {
            var marketplace = new Marketplace(TestDbConnectionString);
            //Act
            var result = await marketplace.SearchAsync("audi");
            //Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.EqualTo(2));

            audiA4.ToExpectedObject().ShouldEqual(result[0]);
            audiA8.ToExpectedObject().ShouldEqual(result[1]);
        }

        [Test]
        public async Task Should_search_by_brand_and_name()
        {
            //Act
            var marketplace = new Marketplace(TestDbConnectionString);
            var result = await marketplace.SearchAsync("audi", "a8");
            //Assert
            audiA8.ToExpectedObject().ShouldEqual(result);
        }

        [Test]
        public async Task Should_list_all_cars()
        {
            //Act
            var marketplace = new Marketplace(TestDbConnectionString);
            var result = await marketplace.ListCarsAsync();
            //Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.EqualTo(3));

            audiA4.ToExpectedObject().ShouldEqual(result[0]);
            audiA8.ToExpectedObject().ShouldEqual(result[1]);
            bmw520d.ToExpectedObject().ShouldEqual(result[2]);
        }

        [Test]
        public async Task Should_buy_a_car()
        {
            var marketplace = new Marketplace(TestDbConnectionString);
            var userId = Guid.NewGuid().ToString();
            //Act
            var result = await marketplace.PurchaseAsync("audi", "a4", userId);
            //Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Brand, Is.EqualTo("audi"));
            Assert.That(result.Name, Is.EqualTo("a4"));
            Assert.That(result.UserId, Is.EqualTo(userId));
            Assert.That(result.Price, Is.EqualTo(audiA4.Price));

            var purchaseHistory = await marketplace.GetPurchaseHistoryAsync(userId);
            Assert.That(purchaseHistory.Length, Is.EqualTo(1));
            Assert.That(purchaseHistory[0].Brand, Is.EqualTo("audi"));
            Assert.That(purchaseHistory[0].Name, Is.EqualTo("a4"));
            Assert.That(purchaseHistory[0].UserId, Is.EqualTo(userId));
            Assert.That(purchaseHistory[0].Price, Is.EqualTo(19990.0m));

        }

        [Test]
        public async Task Should_not_buy_nonexisting_car()
        {
            var marketplace = new Marketplace(TestDbConnectionString);
            var userId = Guid.NewGuid().ToString();
            //Act
            var result = await marketplace.PurchaseAsync("lexus", "rx350", userId);
            //Assert
            Assert.That(result, Is.Null);

            var purchaseHistory = await marketplace.GetPurchaseHistoryAsync(userId);
            Assert.That(purchaseHistory.Length, Is.EqualTo(0));
        }

        [Test]
        public async Task Should_build_sales_report()
        {
            var marketplace = new Marketplace(TestDbConnectionString);

            await marketplace.PurchaseAsync("audi", "a4", Guid.NewGuid().ToString());
            await marketplace.PurchaseAsync("audi", "a4", Guid.NewGuid().ToString());
            await marketplace.PurchaseAsync("bmw", "520d", Guid.NewGuid().ToString());
            await marketplace.PurchaseAsync("bmw", "520d", Guid.NewGuid().ToString());
            await marketplace.PurchaseAsync("bmw", "520d", Guid.NewGuid().ToString());
            await marketplace.PurchaseAsync("bmw", "520d", Guid.NewGuid().ToString());
            //Act
            var result = await marketplace.GetSalesReportAsync();
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
