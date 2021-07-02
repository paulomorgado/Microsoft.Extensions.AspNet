using System;

namespace Microsoft.AspNet.Hosting
{
    /// <summary>
    /// Contains extensions for an <see cref="IHttpRuntimeHostBuilder"/>.
    /// </summary>
    public static class HttpRuntimeHostWebHostBuilderExtensions
    {
        /// <summary>
        /// Adds and configures an ASP.NET Core web application.
        /// </summary>
        public static IHttpRuntimeHostBuilder ConfigureWebHost(this IHttpRuntimeHostBuilder builder, Action<IHttpRuntimeWebHostBuilder> configure)
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
        public static IHttpRuntimeHostBuilder ConfigureWebHost(this IHttpRuntimeHostBuilder builder, Action<IHttpRuntimeWebHostBuilder> configure, Action<HttpRuntimeWebHostBuilderOptions> configureHttpRuntimeWebHostBuilder)
        {
            if (configure is null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            if (configureHttpRuntimeWebHostBuilder is null)
            {
                throw new ArgumentNullException(nameof(configureHttpRuntimeWebHostBuilder));
            }

            var options = new HttpRuntimeWebHostBuilderOptions();
            configureHttpRuntimeWebHostBuilder(options);
            var httpRuntimeWebHostBuilder = new HttpRuntimeWebHostBuilder(builder, options);
            configure(httpRuntimeWebHostBuilder);
            return builder;
        }
    }
}
