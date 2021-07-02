using System;
using System.Web.UI;
using Microsoft.Extensions.Options;
using SampleWebApplication.Options;

namespace SampleWebApplication
{
    public partial class SiteMaster : MasterPage
    {
        public SiteMaster(IOptions<ApplicationOptions> applicationOptions)
        {
            this.ApplicationOptions = applicationOptions.Value;
        }

        protected ApplicationOptions ApplicationOptions { get; }

        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}