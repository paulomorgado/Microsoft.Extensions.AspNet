using System;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNet.Hosting
{
    public static class HttpRuntimeHostBuilderExtensions
    {
        /// <summary>
        /// Allows configuring the <paramref name="builder"/> as a <see cref="IHostBuilder"/>.
        /// </summary>
        /// <param name="builder">The existing builder to configure.</param>
        /// <param name="configureHostBuilder">The delegate wich configures the builder.</param>
        /// <returns></returns>
        public static IHttpRuntimeHostBuilder ConfigureHost(this IHttpRuntimeHostBuilder builder, Action<IHostBuilder> configureHostBuilder)
        {
            if (builder is HttpRuntimeHostBuilder httpRuntimeHostBuilder)
            {
                configureHostBuilder(httpRuntimeHostBuilder);

                return builder;
            }

            throw new ArgumentException($"'{nameof(builder)}' must be of type '{nameof(HttpRuntimeHostBuilder)}'", nameof(builder));
        }

        /// <summary>
        /// Configures an existing <see cref="IHttpRuntimeHostBuilder"/> instance with pre-configured defaults. This will overwrite
        /// previously configured values and is intended to be called before additional configuration calls.
        /// </summary>
        /// <remarks>
        ///   The following defaults are applied to the returned <see cref="HostBuilder"/>:
        ///   <list type="bullet">
        ///     <item><description>set the <see cref="IHostEnvironment.ContentRootPath"/> to the result of <see cref="HttpRuntime.AppDomainAppPath"/></description></item>
        ///     <item><description>load host <see cref="IConfiguration"/> from "DOTNET_" prefixed environment variables</description></item>
        ///     <item><description>load app <see cref="IConfiguration"/> from 'appsettings.json' and 'appsettings.[<see cref="IHostEnvironment.EnvironmentName"/>].json'</description></item>
        ///     <item><description>load app <see cref="IConfiguration"/> from User Secrets when <see cref="IHostEnvironment.EnvironmentName"/> is 'Development' using the entry assembly</description></item>
        ///     <item><description>load app <see cref="IConfiguration"/> from environment variables</description></item>
        ///     <item><description>configure the <see cref="ILoggerFactory"/> to log to the console, debug, and event source output</description></item>
        ///     <item><description>enables scope validation on the dependency injection container when <see cref="IHostEnvironment.EnvironmentName"/> is 'Development'</description></item>
        ///   </list>
        /// </remarks>
        /// <param name="builder">The existing builder to configure.</param>
        /// <param name="args">The command line args.</param>
        /// <returns>The same instance of the <see cref="IHttpRuntimeHostBuilder"/> for chaining.</returns>
        public static IHttpRuntimeHostBuilder ConfigureDefaults(this IHttpRuntimeHostBuilder builder, string[] args = null)
        {
            builder.ConfigureHost(hostBuilder =>
            {
                hostBuilder
                    .ConfigureHostConfiguration(config =>
                    {
                        config.AddEnvironmentVariables(prefix: "DOTNET_");
                        if (args != null && args.Length > 0)
                        {
                            config.AddCommandLine(args);
                        }
                    })
                    .ConfigureAppConfiguration((hostingContext, config) =>
                    {
                        IHostEnvironment env = hostingContext.HostingEnvironment;
                        bool reloadOnChange = GetReloadConfigOnChangeValue(hostingContext);

                        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: reloadOnChange)
                                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: reloadOnChange);

                        if (env.IsDevelopment() && !string.IsNullOrEmpty(env.ApplicationName))
                        {
                            var appAssembly = Assembly.Load(new AssemblyName(env.ApplicationName));
                            if (!(appAssembly is null))
                            {
                                config.AddUserSecrets(appAssembly, optional: true, reloadOnChange: reloadOnChange);
                            }
                        }

                        config.AddEnvironmentVariables();
                        if (args != null && args.Length > 0)
                        {
                            config.AddCommandLine(args);
                        }
                    })
                    .ConfigureLogging((hostingContext, logging) =>
                    {
                        logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                        logging.AddDebug();
                        logging.AddEventSourceLogger();
                        logging.AddEventLog();

                        logging.Configure(options =>
                        {
                            options.ActivityTrackingOptions =
                                ActivityTrackingOptions.SpanId |
                                ActivityTrackingOptions.TraceId |
                                ActivityTrackingOptions.ParentId;
                        });
                    })
                    .UseDefaultServiceProvider((context, options) =>
                    {
                        bool isDevelopment = context.HostingEnvironment.IsDevelopment();
                        options.ValidateScopes = isDevelopment;
                        options.ValidateOnBuild = isDevelopment;
                    });
            });

            return builder;

            bool GetReloadConfigOnChangeValue(HostBuilderContext hostingContext) => hostingContext.Configuration.GetValue("hostBuilder:reloadConfigOnChange", defaultValue: true);
        }

        public static IHttpRuntimeHostBuilder ConfigureHttpRuntimeHostDefaults(this IHttpRuntimeHostBuilder builder, Action<IHttpRuntimeWebHostBuilder> configure)
        {
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
