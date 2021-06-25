using System;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;

namespace SampleWebApplication.Services
{
    public interface ISyndicationClient
    {
        Task<SyndicationFeed> GetSyndicationFeedAsync(Uri syndicationFeedUri);
    }
}