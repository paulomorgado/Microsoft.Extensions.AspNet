using System;
using System.Web;

namespace Microsoft.AspNet.Hosting
{
    public class HttpRuntimeHostHttpApplication : global::System.Web.HttpApplication
    {
        private static IHttpRuntimeHost host;

        public HttpRuntimeHostHttpApplication()
        {
            host?.Start(this);
        }

        public override void Init()
        {
            if (host is null)
            {
                throw new HttpException("The HttpRuntimeHost hasn't been initialized. Invoke BuildHost() in Application_Start.");
            }

            base.Init();
        }

        protected virtual void BuildHost(IHttpRuntimeHostBuilder builder)
        {
        }

        protected virtual void BuildAndStartHost()
        {
            if (!(host is null))
            {
                throw new HttpException("The HttpRuntimeHost has already been initialized.");
            }

            var builder = HttpRuntimeHost.CreateDefaultBuilder();

            BuildHost(builder);

            host = builder.Build();

            host.Start(this);
        }

        protected virtual void StopHost()
        {
            if (!(host is null))
            {
                host.Stop();
                host.Dispose();
            }
        }

        protected virtual void Application_Start(object sender, EventArgs e)
        {
            this.BuildAndStartHost();
        }

        protected virtual void Application_End(object sender, EventArgs e)
        {
            StopHost();
        }
    }
}
