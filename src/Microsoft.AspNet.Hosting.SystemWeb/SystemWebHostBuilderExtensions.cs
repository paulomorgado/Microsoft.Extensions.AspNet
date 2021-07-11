using System;
using System.Collections.Specialized;
using System.Web.Configuration;
using Microsoft.AspNet.Hosting.SystemWeb.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.ConfigurationManager;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Console;

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
                .UseContentRoot(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath)
                .ConfigureHostConfiguration(configurationBuilder =>
                    configurationBuilder.AddConfigurationManager(prefix: "dotnet:", skipConnectionStrings: true))
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

            return builder
                .ConfigureLogging(loggingBuilder =>
                {
                    // Remove Console Logger
                    for (int i = loggingBuilder.Services.Count - 1; i >= 0; i--)
                    {
                        ServiceDescriptor descriptor = loggingBuilder.Services[i];
                        if (descriptor.ImplementationType == typeof(ConsoleLoggerProvider))
                        {
                            loggingBuilder.Services.RemoveAt(i);
                        }
                    }
                })
                .ConfigureWebHost(webHostBuilder =>
                {
                    SystemWebWebHost.ConfigureSystemWebWebDefaults(webHostBuilder);

                    configure?.Invoke(webHostBuilder);
                });
        }
    }
}
