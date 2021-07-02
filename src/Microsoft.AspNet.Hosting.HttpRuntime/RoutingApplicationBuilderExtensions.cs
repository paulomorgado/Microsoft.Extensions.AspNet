using System;
using System.Web.Routing;
using Microsoft.HttpRuntime.Hosting;

namespace Microsoft.AspNet.Hosting.HttpRuntime
{
    /// <summary>
    /// Contains extension methods to <see cref="IHttpRuntimeApplicationBuilder"/>.
    /// </summary>
    public static class RoutingApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds routing to the <see cref="IApplicationBuilder"/> request execution pipeline.
        /// </summary>
        /// <param name="app">The <see cref="IHttpRuntimeApplicationBuilder"/>.</param>
        /// <param name="configureDelegate">Delegate to configure the routes.</param>
        /// <returns>The <see cref="IHttpRuntimeApplicationBuilder"/> so that additional calls can be chained.</returns>
        public static IHttpRuntimeApplicationBuilder UseRouting(
            this IHttpRuntimeApplicationBuilder app,
            Action<RouteCollection> configureDelegate)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (configureDelegate == null)
            {
                throw new ArgumentNullException(nameof(configureDelegate));
            }

            configureDelegate(RouteTable.Routes);

            return app;
        }
    }
}
