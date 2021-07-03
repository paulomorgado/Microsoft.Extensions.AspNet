using System;
using System.Web.Http.Tracing;
using Microsoft.AspNet.Hosting.HttpRuntime.WebApi.Logging;
using Microsoft.AspNet.Hosting.SystemWeb.WebApi;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up <see cref="System.Web.Http.HttpServer"/> services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class WebApiServiceCollectionExtensions
    {
        /// <summary>
        /// Adds MVC services to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <returns>An <see cref="IMvcBuilder"/> that can be used to further configure the MVC services.</returns>
        public static IWebApiBuilder AddWebApi(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAddSingleton<ITraceWriter, LoggerTraceWritter>();


            var builder = new WebApiBuilder(services);

            return builder;
        }
    }
}
