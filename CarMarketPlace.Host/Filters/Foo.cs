using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Filters;
using log4net;

namespace CarMarketPlace.Filters
{
    public class ExceptionHandlingFilter : ExceptionFilterAttribute
    {
        private ILog log = LogManager.GetLogger("Global");

        public override void OnException(HttpActionExecutedContext context)
        {
            log.Error(context.Exception);

            throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("An error occurred, please try again or contact the administrator."),
                ReasonPhrase = "Critical Exception"
            });
        }
    }
}