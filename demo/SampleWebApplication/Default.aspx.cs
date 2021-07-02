using System;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Web.UI;
using Microsoft.Extensions.Options;
using SampleWebApplication.Options;
using SampleWebApplication.Services;

namespace SampleWebApplication
{
    public partial class Default : System.Web.UI.Page
    {
        private readonly ISyndicationClient syndicationClient;
        private readonly ApplicationOptions applicationOptions;
        private readonly FeedOptions feedOptions;

        public Default(ISyndicationClient syndicationClient, IOptions<ApplicationOptions> applicationOptions, IOptions<FeedOptions> feedOptions)
        {
            this.syndicationClient = syndicationClient;
            this.applicationOptions = applicationOptions.Value;
            this.feedOptions = feedOptions.Value;
        }

        protected SyndicationFeed SyndicationFeed { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            this.RegisterAsyncTask(new PageAsyncTask(LoadSyndicationFeedAsync));
        }

        private async Task LoadSyndicationFeedAsync()
        {
            this.SyndicationFeed = await this.syndicationClient.GetSyndicationFeedAsync(this.feedOptions.Uri);
            this.FeedItemsDataList.DataSource = this.SyndicationFeed.Items;
            this.DataBind();
        }
    }
}