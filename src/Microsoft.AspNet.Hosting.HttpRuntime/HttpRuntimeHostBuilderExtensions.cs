using System;
using Microsoft.AspNet.Hosting.HttpRuntime.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

        public static IHostBuilder ConfigureHttpRuntimeWebHostDefaults(this IHostBuilder builder, Action<IWebHostBuilder> configure)
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
