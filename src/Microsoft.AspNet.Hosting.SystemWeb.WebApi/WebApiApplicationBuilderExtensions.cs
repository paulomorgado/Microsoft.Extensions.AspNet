using System;
using System.Web.Http;
using Microsoft.AspNet.Hosting.SystemWeb.WebApi.DependencyInjection;
using Microsoft.AspNetCore.Builder;

namespace Microsoft.AspNet.Hosting.SystemWeb.WebApi
{
    /// <summary>
    /// Contains extension methods to <see cref="IApplicationBuilder"/>.
    /// </summary>
    public static class WebApiApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds Web API to the <see cref="IApplicationBuilder"/> request execution pipeline.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/>.</param>
        /// <param name="configureDelegate">Delegate to configure Web API.</param>
        /// <returns>The <see cref="IApplicationBuilder"/> so that additional calls can be chained.</returns>
        public static IApplicationBuilder UseWebApi(this IApplicationBuilder app, Action<IWebApiApplicationBuilder> configureDelegate)
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
