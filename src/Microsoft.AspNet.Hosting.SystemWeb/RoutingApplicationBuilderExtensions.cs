using System;
using System.Web.Routing;
using Microsoft.AspNetCore.Builder;

namespace Microsoft.AspNet.Hosting.SystemWeb
{
    /// <summary>
    /// Contains extension methods to <see cref="IApplicationBuilder"/>.
    /// </summary>
    public static class RoutingApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds routing to the <see cref="IApplicationBuilder"/> request execution pipeline.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/>.</param>
        /// <param name="configureDelegate">Delegate to configure the routes.</param>
        /// <returns>The <see cref="IApplicationBuilder"/> so that additional calls can be chained.</returns>
        public static IApplicationBuilder UseRouting(
            this IApplicationBuilder app,
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
