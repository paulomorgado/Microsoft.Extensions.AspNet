using System;
using System.Net.Http;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.Logging;

namespace SampleWebApplication.Services
{
    public sealed class SyndicationClient : ISyndicationClient
    {
        private static readonly XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
        private readonly HttpClient httpClient;
        private readonly ILogger<SyndicationClient> logger;

        public SyndicationClient(HttpClient httpClient, ILogger<SyndicationClient> logger)
        {
            this.httpClient = httpClient;
            this.logger = logger;
        }

        public async Task<SyndicationFeed> GetSyndicationFeedAsync(Uri syndicationFeedUri)
        {
            this.logger.LogInformation(new EventId(1, "GetSyndicationFeed"), "Getting syndication feed from '{uri}'", syndicationFeedUri);

            using (var response = await this.httpClient.GetAsync(syndicationFeedUri))
            {
                response.EnsureSuccessStatusCode();

                using (var stream = await response.Content.ReadAsStreamAsync())
{
                    using (var xmlReader = XmlReader.Create(stream, xmlReaderSettings))
                    {
                        return SyndicationFeed.Load(xmlReader);
                    }
                }
            }
        }

        public void Dispose()
        {
            this.httpClient.Dispose();
        }
    }
}