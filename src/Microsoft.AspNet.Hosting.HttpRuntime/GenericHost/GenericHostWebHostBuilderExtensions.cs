using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Microsoft.AspNet.Hosting
{
    /// <summary>
    /// Contains extensions for an <see cref="IHostBuilder"/>.
    /// </summary>
    public static class GenericHostWebHostBuilderExtensions
    {
        /// <summary>
        /// Adds and configures an ASP.NET Core web application.
        /// </summary>
        public static IHostBuilder ConfigureWebHost(this IHostBuilder builder, Action<IWebHostBuilder> configure)
        {
            if (configure is null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            return builder.ConfigureWebHost(configure, _ => { });
        }

        /// <summary>
        /// Adds and configures an ASP.NET Core web application.
        /// </summary>
        public static IHostBuilder ConfigureWebHost(this IHostBuilder builder, Action<IWebHostBuilder> configure, Action<GenericWebHostBuilderOptions> configureHttpRuntimeWebHostBuilder)
        {
            if (configure is null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            if (configureHttpRuntimeWebHostBuilder is null)
            {
                throw new ArgumentNullException(nameof(configureHttpRuntimeWebHostBuilder));
            }

            var options = new GenericWebHostBuilderOptions();
            configureHttpRuntimeWebHostBuilder(options);
            var httpRuntimeWebHostBuilder = new GenericWebHostBuilder(builder, options);
            configure(httpRuntimeWebHostBuilder);
            builder.ConfigureServices((context, services) => services.AddHostedService<GenericWebHostService>());
            return builder;
        }
    }
}
