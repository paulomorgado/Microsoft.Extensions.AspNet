using System;
using System.ServiceModel;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.ServiceModel.DependencyInjection;
using SampleWebApplication.Options;
using SampleWebApplication.Services;

namespace SampleWebApplication.ServiceModel
{
    [SystemWebHostServiceBehavior()]
    public class Feed : IFeed
    {
        private readonly ISyndicationClient syndicationClient;
        private readonly FeedOptions options;

        public Feed(ISyndicationClient syndicationClient, IOptions<FeedOptions> options)
        {
            this.syndicationClient = syndicationClient ?? throw new ArgumentNullException(nameof(syndicationClient));
            this.options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<string> GetFeedAsync()
        {
            var feed = await syndicationClient.GetSyndicationFeedAsync(options.Uri);

            return feed.Title.Text;
        }
    }
}
