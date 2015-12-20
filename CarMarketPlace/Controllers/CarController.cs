using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using CarMarketPlace.Models;
using MarketPlaceService;

namespace CarMarketPlace.Controllers
{
    [Authorize]
    [RoutePrefix("api/cars")]
    [AllowAnonymous]
    public class CarController : ApiController
    {
        private readonly ICarService carService;

        public CarController(ICarService carService)
        {
            this.carService = carService;
        }

        public CarController()
        {
            var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            this.carService = new Marketplace(connectionString);
        }

        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> AddAsync(CarModel carModel)
        {
            var car = ToCar(carModel);
            var added = await carService.Add(car);

            if (!added)
                return Conflict();

            var location = ToLocation(car);
            return Created(new Uri(location), carModel);
        }

        [HttpPut]
        [Route("{brand}/{name}")]
        public async Task<IHttpActionResult> UpdateAsync([FromUri]string brand, [FromUri]string name, [FromBody]decimal price)
        {
            var car = new Car
            {
                Brand = brand,
                Name = name,
                Price = price
            };
            var updated = await carService.Update(car);

            if (!updated)
                return NotFound();

            return Ok();
        }

        [HttpDelete]
        [Route("{brand}/{name}")]
        public async Task<IHttpActionResult> DeleteAsync([FromUri]string brand, [FromUri]string name)
        {
            var delete = await carService.Delete(brand, name);

            if (!delete)
                return NotFound();

            return Ok();
        }

        [HttpGet]
        [Route("{brand}/{name}")]
        public async Task<IHttpActionResult> SearchAsync([FromUri]string brand, [FromUri]string name)
        {
            var searchResult = await carService.SearchAsync(brand, name);

            if (searchResult == null)
                return NotFound();

            return Content(HttpStatusCode.Found, searchResult);
        }

        [HttpGet]
        [Route("{brand}")]
        public async Task<IHttpActionResult> SearchAsync(string brand)
        {
            var searchResult = await carService.SearchAsync(brand);

            if (searchResult.Length == 0)
                return NotFound();

            return Content(HttpStatusCode.Found, searchResult);
        }

        private string ToLocation(Car car)
        {
            return Request.RequestUri + "/" + car.Brand + "/" + car.Name;
        }

        private static Car ToCar(CarModel carModel)
        {
            var car = new Car
            {
                Brand = carModel.Brand,
                Name = carModel.Name,
                Price = carModel.Price
            };
            return car;
        }
    }
}
