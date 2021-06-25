using System;

namespace Microsoft.AspNet.Hosting
{
    public class HttpApplicationHost<TStartup>: HttpRuntimeHostHttpApplication
        where TStartup : class
    {
        protected override void BuildHost(IHttpRuntimeHostBuilder builder)
        {
            builder.ConfigureHttpRuntimeHostDefaults(webBuilder => webBuilder.UseStartup<TStartup>());
            base.BuildHost(builder);
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
