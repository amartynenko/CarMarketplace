using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using CarMarketPlace.Controllers;
using CarMarketPlace.Models;
using ExpectedObjects;
using MarketPlaceService;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class CarControllerTests : DbFixture
    {
        private Marketplace marketplace;
        private CarController controller;

        protected override void DoSetUp()
        {
            marketplace = new Marketplace(TestDbConnectionString);

            controller = new CarController(marketplace);
            controller.Request = new HttpRequestMessage(HttpMethod.Post, "https://localhost:1080/api/car");
            controller.Request.SetConfiguration(new HttpConfiguration());
        }

        [Test]
        public async Task Should_return_correct_result_and_content_when_adding_a_car()
        {
            var car = new CarModel { Brand = "honda", Name = "accord", Price = 24500.0m };
            //Act
            var result = await controller.AddAsync(car) as CreatedNegotiatedContentResult<CarModel>;
            //Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Content, Is.Not.Null);
            Assert.That(result.Content.Brand, Is.EqualTo("honda"));
            Assert.That(result.Content.Name, Is.EqualTo("accord"));
            Assert.That(result.Content.Price, Is.EqualTo(24500.0m));
        }

        [Test]
        public async Task Should_return_conflict_when_adding_a_duplicate_car()
        {
            var car = new CarModel { Brand = "honda", Name = "accord", Price = 24500.0m };
            await controller.AddAsync(car);
            //Act
            var anotherResult = await controller.AddAsync(car) as ConflictResult;
            //Assert
            Assert.That(anotherResult, Is.Not.Null);
        }

        [Test]
        public async Task Should_store_car_when_adding_a_car()
        {
            var carModel = new CarModel { Brand = "honda", Name = "accord", Price = 24500.0m };
            //Act
            await controller.AddAsync(carModel);
            //Assert
            var car = await marketplace.SearchAsync("honda", "accord");
            Assert.That(car, Is.Not.Null);
            Assert.That(car.Brand, Is.EqualTo("honda"));
            Assert.That(car.Name, Is.EqualTo("accord"));
            Assert.That(car.Price, Is.EqualTo(24500.0m));
        }

        [Test]
        public async Task Should_return_correct_result_and_update_data_when_updating_a_car()
        {
            var car = new CarModel { Brand = "honda", Name = "accord", Price = 24500.0m };
            await controller.AddAsync(car);

            //Act
            var result = await controller.UpdateAsync("honda", "accord", 33333.0m) as OkResult;
            //Assert
            Assert.That(result, Is.Not.Null);

            var found = await marketplace.SearchAsync("honda", "accord");
            Assert.That(found, Is.Not.Null);
            Assert.That(found.Brand, Is.EqualTo("honda"));
            Assert.That(found.Name, Is.EqualTo("accord"));
            Assert.That(found.Price, Is.EqualTo(33333.0m));
        }

        [Test]
        public async Task Should_return_notfound_when_updating_a_nonexisting_car()
        {
            //Act
            var result = await controller.UpdateAsync("honda", "accord", 33333.0m) as NotFoundResult;
            //Assert
            Assert.That(result, Is.Not.Null);

            var found = await marketplace.SearchAsync("honda", "accord");
            Assert.That(found, Is.Null);
        }

        [Test]
        public async Task Should_return_correct_result_and_delete_data_when_deleting_a_car()
        {
            var car = new CarModel { Brand = "honda", Name = "accord", Price = 24500.0m };
            await controller.AddAsync(car);

            //Act
            var result = await controller.DeleteAsync("honda", "accord") as OkResult;
            //Assert
            Assert.That(result, Is.Not.Null);

            var found = await marketplace.SearchAsync("honda", "accord");
            Assert.That(found, Is.Null);
        }

        [Test]
        public async Task Should_return_notfound_when_deleting_a_nonexisting_car()
        {
            //Act
            var result = await controller.DeleteAsync("honda", "accord") as NotFoundResult;
            //Assert
            Assert.That(result, Is.Not.Null);

            var found = await marketplace.SearchAsync("honda", "accord");
            Assert.That(found, Is.Null);
        }

        [Test]
        public async Task Should_return_correct_result_when_searching_a_car()
        {
            var car = new CarModel { Brand = "honda", Name = "accord", Price = 24500.0m };
            await controller.AddAsync(car);

            //Act
            var result = await controller.SearchAsync("honda", "accord") as NegotiatedContentResult<Car>;
            //Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Content, Is.Not.Null);
            Assert.That(result.Content.Brand, Is.EqualTo("honda"));
            Assert.That(result.Content.Name, Is.EqualTo("accord"));
            Assert.That(result.Content.Price, Is.EqualTo(24500.0m));
        }

        [Test]
        public async Task Should_return_notfound_when_searching_a_nonexisting_car()
        {
            //Act
            var result = await controller.SearchAsync("lexus", "accord") as NotFoundResult;
            //Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public async Task Should_return_correct_result_when_searching_a_car_by_brand()
        {
            var car1 = new CarModel { Brand = "cherry", Name = "qq", Price = 7899 };
            var car2 = new CarModel { Brand = "cherry", Name = "qq42", Price = 24500.0m };

            await controller.AddAsync(car1);
            await controller.AddAsync(car2);

            //Act
            var result = await controller.SearchAsync("cherry") as NegotiatedContentResult<Car[]>;
            //Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Content, Is.Not.Null);
            Assert.That(result.Content.Length, Is.EqualTo(2));
            car1.ToExpectedObject().ShouldMatch(result.Content[0]);
            car2.ToExpectedObject().ShouldMatch(result.Content[1]);
        }

        [Test]
        public async Task Should_return_notfound_when_searching_a_nonexisting_car_by_brand()
        {
            //Act
            var result = await controller.SearchAsync("lexus") as NotFoundResult;
            //Assert
            Assert.That(result, Is.Not.Null);
        }
    }
}
