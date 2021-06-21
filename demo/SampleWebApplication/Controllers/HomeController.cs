using Microsoft.Extensions.Options;
using SampleWebApplication.Options;
using SampleWebApplication.Services;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SampleWebApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ISyndicationClient syndicationClient;
        private readonly ApplicationOptions applicationOptions;
        private readonly FeedOptions feedOptions;

        public HomeController(ISyndicationClient syndicationClient, IOptions<ApplicationOptions> applicationOptions, IOptions<FeedOptions> feedOptions)
        {
            this.syndicationClient = syndicationClient;
            this.applicationOptions = applicationOptions.Value;
            this.feedOptions = feedOptions.Value;
        }

        public async Task<ActionResult> Index()
        {
            var feed = await this.syndicationClient.GetSyndicationFeedAsync(this.feedOptions.Uri);
            ViewBag.ApplicationName = this.applicationOptions.Name;
            return View(feed);
        }
    }
}