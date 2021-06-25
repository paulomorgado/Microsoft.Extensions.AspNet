using System;
using System.Web.Http;
using Microsoft.Extensions.DependencyInjection.AspNetWebApi;
using Microsoft.HttpRuntime.Hosting;

namespace Microsoft.AspNet.Hosting.HttpRuntime.WebApi
{
    /// <summary>
    /// Contains extension methods to <see cref="IHttpRuntimeApplicationBuilder"/>.
    /// </summary>
    public static class WebApiApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds Web API to the <see cref="IHttpRuntimeApplicationBuilder"/> request execution pipeline.
        /// </summary>
        /// <param name="app">The <see cref="IHttpRuntimeApplicationBuilder"/>.</param>
        /// <param name="configureDelegate">Delegate to configure Web API.</param>
        /// <returns>The <see cref="IHttpRuntimeApplicationBuilder"/> so that additional calls can be chained.</returns>
        public static IHttpRuntimeApplicationBuilder UseWebApi(this IHttpRuntimeApplicationBuilder app, Action<IWebApiApplicationBuilder> configureDelegate)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            var builder = new WebApiApplicationBuilder(System.Web.HttpRuntime.WebObjectActivator);

            builder.Configure(config => config.DependencyResolver = new WebApiServiceProviderDependencyResolver(System.Web.HttpRuntime.WebObjectActivator));

            configureDelegate(builder);

            GlobalConfiguration.Configure(builder.Configure);

            return app;
        }
    }
}
