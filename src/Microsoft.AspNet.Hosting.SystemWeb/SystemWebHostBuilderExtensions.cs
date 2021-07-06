using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;
using Microsoft.AspNet.Hosting.SystemWeb.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Microsoft.AspNet.Hosting.SystemWeb
{
    public static class SystemWebHostBuilderExtensions
    {
        public static IHostBuilder ConfigureSystemWebHostDefaults(this IHostBuilder builder)
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
                .UseServiceProviderFactory(new SystemWebServiceProviderFactory());
        }

        public static IHostBuilder ConfigureSystemWebWebHostDefaults(this IHostBuilder builder, Action<IWebHostBuilder> configure)
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
                SystemWebWebHost.ConfigureSystemWebWebDefaults(webHostBuilder);

                configure?.Invoke(webHostBuilder);
            });
        }
    }
}
