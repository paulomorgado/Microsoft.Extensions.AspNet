using System;
using System.Web;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.Hosting.HttpRuntime
{
    internal sealed class HttpRuntimeHostModule : IHttpModule
    {
        private readonly IServiceScope httpApplicationServiceScope;

        public HttpRuntimeHostModule(IServiceProvider serviceProvider)
        {
            httpApplicationServiceScope = serviceProvider.CreateScope();
        }

        public void Init(HttpApplication httpApplication)
        {
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
    }
}
