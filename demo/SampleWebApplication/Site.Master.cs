using Microsoft.Extensions.Options;
using SampleWebApplication.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

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