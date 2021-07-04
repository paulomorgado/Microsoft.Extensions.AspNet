using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;
using Microsoft.AspNet.Hosting.HttpRuntime.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
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
                .ConfigureHostConfiguration(configurationBuilder =>
                    configurationBuilder.AddInMemoryCollection(
                        from key in WebConfigurationManager.AppSettings.AllKeys
                        let value = WebConfigurationManager.AppSettings.GetValues(key)?.LastOrDefault()
                        where !(value is null)
                        select new KeyValuePair<string, string>(key, value)))
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
