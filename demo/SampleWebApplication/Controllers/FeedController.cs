using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Extensions.Options;
using SampleWebApplication.Options;
using SampleWebApplication.Services;

namespace SampleWebApplication.Controllers
{
    [RoutePrefix("api/feed")]
    public class FeedController : ApiController
    {
        private readonly ISyndicationClient syndicationClient;
        private readonly FeedOptions options;

        public FeedController(ISyndicationClient syndicationClient, IOptions<FeedOptions> feedOptions)
        {
            this.syndicationClient = syndicationClient;
            this.options = feedOptions.Value;
        }

        // GET api/feed
        public async Task<IHttpActionResult> Get()
        {
            var feed = await this.syndicationClient.GetSyndicationFeedAsync(this.options.Uri);

            return Ok(new
            {
                Title = feed.Title.Text,
                Link = feed.Links[0].Uri,
                Items = from item in feed.Items
                        select new
                        {
                            Title = item.Title.Text,
                            Link = item.Links[0].Uri,
                            Summary = item.Summary.Text,
                        },
            });
        }
    }
}
