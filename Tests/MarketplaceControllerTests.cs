using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using CarMarketPlace.Controllers;
using MarketPlaceService;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class MarketplaceControllerTests : DbFixture
    {
        private Marketplace marketplace;
        private MarketplaceController controller;

        protected override void DoSetUp()
        {
            marketplace = new Marketplace(TestDbConnectionString);

            controller = new MarketplaceController(marketplace);
            controller.Request = new HttpRequestMessage(HttpMethod.Post, "https://localhost:1080/api/car");
            controller.Request.SetConfiguration(new HttpConfiguration());

            controller.User = new GenericPrincipal
            (
               new GenericIdentity("anton@gmail.com", "email"),
               new[] { "managers", "executives" }
            );
        }

        [Test]
        public async Task Should_return_purchase_when_purchasing_a_car()
        {
            var car = new Car { Brand = "honda", Name = "accord", Price = 24500.0m };
            await marketplace.AddAsync(car);
            //Act
            var result = await controller.PurchaseAsync("honda", "accord") as CreatedNegotiatedContentResult<Purchase>;
            //Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Content, Is.Not.Null);
            Assert.That(result.Content.Brand, Is.EqualTo("honda"));
            Assert.That(result.Content.Name, Is.EqualTo("accord"));
            Assert.That(result.Content.Price, Is.EqualTo(24500.0m));
            Assert.That(result.Content.UserId, Is.EqualTo("anton@gmail.com"));
        }

        [Test]
        public async Task Should_return_not_found_when_purchasing_a_nonexisting_car()
        {
            var car = new Car { Brand = "honda", Name = "accord", Price = 24500.0m };
            await marketplace.AddAsync(car);
            //Act
            var result = await controller.PurchaseAsync("ford", "mondeo") as NotFoundResult;
            //Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Should_return_list_of_cars()
        {
            var car1 = new Car { Brand = "honda", Name = "accord", Price = 24500.0m };
            var car2 = new Car { Brand = "ford", Name = "focus", Price = 14500.0m };
            await marketplace.AddAsync(car1);
            await marketplace.AddAsync(car2);
            //Act
            var result = await controller.ListAllAsync() as NegotiatedContentResult<Car[]>;
            //Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Content, Is.Not.Null);
            Assert.That(result.Content.Length, Is.EqualTo(2));
            Assert.That(result.Content.Any(x => x.Brand == "honda" && x.Name == "accord"), Is.True);
            Assert.That(result.Content.Any(x => x.Brand == "ford" && x.Name == "focus"), Is.True);
        }

        [Test]
        public async Task Should_return_purchase_history_when_getting_history()
        {
            var car1 = new Car { Brand = "honda", Name = "accord", Price = 24500.0m };
            var car2 = new Car { Brand = "ford", Name = "focus", Price = 14500.0m };
            await marketplace.AddAsync(car1);
            await marketplace.AddAsync(car2);
            await marketplace.PurchaseAsync("honda", "accord", "bob");
            await marketplace.PurchaseAsync("honda", "accord", "ann");
            await marketplace.PurchaseAsync("honda", "accord", "joe");
            await marketplace.PurchaseAsync("ford", "focus", "jim");
            //Act
            var result = await controller.GetSalesStatsAsync() as NegotiatedContentResult<CarSales[]>;
            //Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Content, Is.Not.Null);
            Assert.That(result.Content.Length, Is.EqualTo(2));
            Assert.That(result.Content[0].Brand, Is.EqualTo("honda"));
            Assert.That(result.Content[0].Name, Is.EqualTo("accord"));
            Assert.That(result.Content[0].Count, Is.EqualTo(3));
            Assert.That(result.Content[1].Brand, Is.EqualTo("ford"));
            Assert.That(result.Content[1].Name, Is.EqualTo("focus"));
            Assert.That(result.Content[1].Count, Is.EqualTo(1));
        }
    }
}
