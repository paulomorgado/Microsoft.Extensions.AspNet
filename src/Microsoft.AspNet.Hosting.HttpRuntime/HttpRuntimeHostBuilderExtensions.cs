using System;
using System.Reflection;
using Microsoft.AspNet.Hosting.HttpRuntime.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNet.Hosting
{
    public static class HttpRuntimeHostBuilderExtensions
    {
        public static IHostBuilder ConfigureHttpRuntimeHostDefaults(this IHostBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder
                .UseContentRoot(System.Web.HttpRuntime.AppDomainAppPath)
                .UseServiceProviderFactory<IServiceCollection>(new HttpRuntimeServiceProviderFactory());
        }

        public static IHostBuilder ConfigureHttpRuntimeWebHostDefaults(this IHostBuilder builder, Action<IHttpRuntimeWebHostBuilder> configure)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (configure is null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            return builder.ConfigureWebHost(webHostBuilder =>
            {
                HttpRuntimeWebHost.ConfigureHttpRuntimeWebDefaults(webHostBuilder);

                configure?.Invoke(webHostBuilder);
            });
        }
    }
}
