using System;
using System.Web;
using Microsoft.AspNet.Hosting.HttpRuntime;
using Microsoft.Extensions.Hosting;

namespace Microsoft.AspNet.Hosting
{
    public abstract class HttpRuntimeHostHttpApplication : global::System.Web.HttpApplication
    {
        private static IHost host;

        protected HttpRuntimeHostHttpApplication()
        {
            if (!(host is null))
            {
                new HttpRuntimeHostModule(host.Services).Init(this);
            }
        }

        public override void Init()
        {
            if (host is null)
            {
                throw new HttpException("The HttpRuntimeHost hasn't been initialized. Invoke BuildHost() in Application_Start.");
            }

            base.Init();
        }

        protected virtual void BuildHost(IHostBuilder builder)
        {
        }

        protected abstract void BuildWebHost(IHttpRuntimeWebHostBuilder webBuilder);

        protected virtual void BuildAndStartHost()
        {
            if (!(host is null))
            {
                throw new HttpException("The HttpRuntimeHost has already been initialized.");
            }

            var builder = Host
                .CreateDefaultBuilder()
                .ConfigureHttpRuntimeHostDefaults();

            BuildHost(builder);

            builder.ConfigureHttpRuntimeWebHostDefaults(BuildWebHost);

            host = builder.Build();

            host.Start();

            new HttpRuntimeHostModule(host.Services).Init(this);
        }

        protected virtual void StopHost()
        {
            if (!(host is null))
            {
                host.StopAsync().GetAwaiter().GetResult();
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
