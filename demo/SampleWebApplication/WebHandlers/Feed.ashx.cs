using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Options;
using SampleWebApplication.Options;
using SampleWebApplication.Services;

namespace SampleWebApplication.WebHandlers
{
    /// <summary>
    /// Summary description for Feed
    /// </summary>
    public class FeedHandler : HttpTaskAsyncHandler
    {
        private readonly ISyndicationClient syndicationClient;
        private readonly FeedOptions options;

        public FeedHandler(ISyndicationClient syndicationClient, IOptions<FeedOptions> feedOptions)
        {
            this.syndicationClient = syndicationClient;
            options = feedOptions.Value;
        }

        public override async Task ProcessRequestAsync(HttpContext context)
        {
            var feed = await syndicationClient.GetSyndicationFeedAsync(options.Uri);

            var response = new XElement("Feed",
                new XElement("Title", feed.Title.Text),
                new XElement("Link", feed.Links[0].Uri),
                new XElement("Items", from item in feed.Items
                                      select new XElement("Item",
                                      new XElement("Title", item.Title.Text),
                                          new XElement("Link", item.Links[0].Uri),
                                          new XElement("Summary", item.Summary.Text)
                                        )
                                    )
                );

            using (var xmlWriter = XmlWriter.Create(context.Response.OutputStream))
            {
                response.WriteTo(xmlWriter);
            }

            context.Response.ContentType = "application/xml";
        }
    }
}