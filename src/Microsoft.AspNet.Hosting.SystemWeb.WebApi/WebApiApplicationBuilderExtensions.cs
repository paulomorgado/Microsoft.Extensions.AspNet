using System;
using System.Web.Http;
using System.Web.Http.Dependencies;
using Microsoft.AspNet.Hosting.SystemWeb.DependencyInjection;
using Microsoft.AspNet.Hosting.SystemWeb.WebApi.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

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

            var builder = new WebApiApplicationBuilder(app.ApplicationServices);

            builder.Configure((services, config) =>
            {
                config.DependencyResolver = services.GetRequiredService<IDependencyResolver>();
            });

            configureDelegate(builder);

            GlobalConfiguration.Configure(builder.Configure);

            return app;
        }
    }
}
