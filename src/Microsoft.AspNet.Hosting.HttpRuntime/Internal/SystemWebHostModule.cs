using System;
using System.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Microsoft.AspNet.Hosting.HttpRuntime
{
    internal sealed class SystemWebHostModule : IHttpModule
    {
        private static readonly Lazy<IHost> host = new Lazy<IHost>(BuildHost);
        private IServiceScope httpApplicationServiceScope;

        public SystemWebHostModule()
        {
        }

        public void Init(HttpApplication httpApplication)
        {
            httpApplicationServiceScope = host.Value.Services.CreateScope();

            httpApplication.BeginRequest += (sender, e) =>
            {
                var contextServicesScope = httpApplicationServiceScope.ServiceProvider.CreateScope();
                httpApplication.Context.Items[HttpContextKeys.HttpContextServiceProviderScopeKey] = contextServicesScope;
                httpApplication.Context.Items[HttpContextKeys.HttpContextServiceProviderKey] = contextServicesScope.ServiceProvider;
            };

            httpApplication.EndRequest += (sender, e) =>
            {
                if (httpApplication.Context.Items[HttpContextKeys.HttpContextServiceProviderScopeKey] is IServiceScope httpContextServiceScope)
                {
                    httpContextServiceScope.Dispose();
                }
            };
        }

        public void Dispose()
        {
            httpApplicationServiceScope.Dispose();
        }

        private static IHost BuildHost()
        {
            var builder = Host
                .CreateDefaultBuilder()
                .ConfigureHttpRuntimeHostDefaults()
                .ConfigureHost()
                .ConfigureHttpRuntimeWebHostDefaults(webBuilder => webBuilder.ConfigureWebHost());

            var host = builder.Build();

            host.Start();

            return host;
        }
    }
}
