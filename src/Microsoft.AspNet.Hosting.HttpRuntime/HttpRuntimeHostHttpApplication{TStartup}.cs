using System;
using Microsoft.AspNetCore.Hosting;

namespace Microsoft.AspNet.Hosting
{
    public class HttpRuntimeHostHttpApplication<TStartup> : HttpRuntimeHostHttpApplication
        where TStartup : class
    {
        public HttpRuntimeHostHttpApplication() : base() { }

        protected override void BuildWebHost(IWebHostBuilder webBuilder)
        {
            webBuilder.UseStartup<TStartup>();
        }

        protected override void Application_Start(object sender, EventArgs e)
        {
            base.Application_Start(sender, e);
        }

        protected override void Application_End(object sender, EventArgs e)
        {
            base.Application_End(sender, e);
        }
    }
}
