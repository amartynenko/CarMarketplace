using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using MarketPlaceService;

namespace CarMarketPlace.Controllers
{
    [Authorize]
    [RoutePrefix("api/marketplace")]
    [AllowAnonymous]
    public class MarketplaceController : ApiController
    {
        private readonly IMarketplace marketplaceService;

        public MarketplaceController(IMarketplace marketplaceService)
        {
            this.marketplaceService = marketplaceService;
        }

        public MarketplaceController()
        {
            var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            marketplaceService = new Marketplace(connectionString);
        }

        [HttpPost]
        [Route("buy/{brand}/{name}")]
        public async Task<IHttpActionResult> PurchaseAsync([FromUri]string brand, [FromUri]string name)
        {
            var purchase = await marketplaceService.PurchaseAsync(brand, name, User.Identity.Name);
            if (purchase == null)
                return NotFound();

            return Created(new Uri(ToLocation(purchase.Brand, purchase.Name)), purchase);
        }

        [HttpGet]
        [Route("salesreport")]
        public async Task<IHttpActionResult> GetSalesStatsAsync()
        {
            var report = await marketplaceService.GetSalesReportAsync();
            return Content(HttpStatusCode.OK, report);
        }

        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> ListAllAsync()
        {
            var searchResult = await marketplaceService.ListCarsAsync();

            return Content(HttpStatusCode.OK, searchResult);
        }

        private string ToLocation(string brand, string name)
        {
            return Request.RequestUri + "/" + brand + "/" + name;
        }
    }
}
