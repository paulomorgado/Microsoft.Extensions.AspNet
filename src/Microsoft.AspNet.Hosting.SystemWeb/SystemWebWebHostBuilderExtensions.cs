﻿using System;
using System.Reflection;
using Microsoft.AspNet.Hosting.SystemWeb.DependencyInjection;
using Microsoft.AspNet.Hosting.SystemWeb.Startup;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNet.Hosting.SystemWeb
{
    /// <summary>
    /// Contains extensions for configuring an <see cref="IWebHostBuilder" />.
    /// </summary>
    public static class SystemWebWebHostBuilderExtensions
    {
        /// <summary>
        /// Specify the startup method to be used to configure the web application.
        /// </summary>
        /// <param name="hostBuilder">The <see cref="IWebHostBuilder"/> to configure.</param>
        /// <param name="configureApp">The delegate that configures the <see cref="IApplicationBuilder"/>.</param>
        /// <returns>The <see cref="IWebHostBuilder"/>.</returns>
        public static IWebHostBuilder Configure(this IWebHostBuilder hostBuilder, Action<IApplicationBuilder> configureApp)
        {
            return hostBuilder.Configure((_, app) => configureApp(app), configureApp.GetMethodInfo().DeclaringType.Assembly.GetName().Name);
        }

        /// <summary>
        /// Specify the startup method to be used to configure the web application.
        /// </summary>
        /// <param name="hostBuilder">The <see cref="IWebHostBuilder"/> to configure.</param>
        /// <param name="configureApp">The delegate that configures the <see cref="IApplicationBuilder"/>.</param>
        /// <returns>The <see cref="IWebHostBuilder"/>.</returns>
        public static IWebHostBuilder Configure(this IWebHostBuilder hostBuilder, Action<WebHostBuilderContext, IApplicationBuilder> configureApp)
        {
            return hostBuilder.Configure(configureApp, configureApp.GetMethodInfo().DeclaringType.Assembly.GetName().Name);
        }

        private static IWebHostBuilder Configure(this IWebHostBuilder hostBuilder, Action<WebHostBuilderContext, IApplicationBuilder> configureApp, string startupAssemblyName)
        {
            if (configureApp == null)
            {
                throw new ArgumentNullException(nameof(configureApp));
            }

            hostBuilder.UseSetting(WebHostDefaults.ApplicationKey, startupAssemblyName);

            // Light up the ISupportsStartup implementation
            if (hostBuilder is ISupportsStartup supportsStartup)
            {
                return supportsStartup.Configure(configureApp);
            }

            return hostBuilder.ConfigureServices((context, services) =>
            {
                services.AddSingleton<IStartup>(sp =>
                {
                    return new DelegateStartup(sp.GetRequiredService<IServiceProviderFactory<IServiceCollection>>(), app => configureApp(context, app));
                });
            });
        }

        /// <summary>
        /// Specify a factory that creates the startup instance to be used by the web host.
        /// </summary>
        /// <param name="hostBuilder">The <see cref="IWebHostBuilder"/> to configure.</param>
        /// <param name="startupFactory">A delegate that specifies a factory for the startup class.</param>
        /// <returns>The <see cref="IWebHostBuilder"/>.</returns>
        /// <remarks>When using the il linker, all public methods of <typeparamref name="TStartup"/> are preserved. This should match the Startup type directly (and not a base type).</remarks>
        public static IWebHostBuilder UseStartup<TStartup>(this IWebHostBuilder hostBuilder, Func<WebHostBuilderContext, TStartup> startupFactory) where TStartup : class
        {
            if (startupFactory == null)
            {
                throw new ArgumentNullException(nameof(startupFactory));
            }

            var startupAssemblyName = startupFactory.GetMethodInfo().DeclaringType.Assembly.GetName().Name;

            hostBuilder.UseSetting(WebHostDefaults.ApplicationKey, startupAssemblyName);

            // Light up the GenericWebHostBuilder implementation
            if (hostBuilder is ISupportsStartup supportsStartup)
            {
                return supportsStartup.UseStartup(startupFactory);
            }

            return hostBuilder
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton(typeof(IStartup), sp =>
                    {
                        var instance = startupFactory(context) ?? throw new InvalidOperationException("The specified factory returned null startup instance.");

                        var hostingEnvironment = sp.GetRequiredService<IWebHostEnvironment>();

                        // Check if the instance implements IHttpRuntimeStartup before wrapping
                        if (instance is IStartup startup)
                        {
                            return startup;
                        }

                        return new ConventionBasedStartup(StartupLoader.LoadMethods(sp, instance.GetType(), hostingEnvironment.EnvironmentName, instance));
                    });
                });
        }

        /// <summary>
        /// Specify the startup type to be used by the web host.
        /// </summary>
        /// <param name="hostBuilder">The <see cref="IWebHostBuilder"/> to configure.</param>
        /// <param name="startupType">The <see cref="Type"/> to be used.</param>
        /// <returns>The <see cref="IWebHostBuilder"/>.</returns>
        public static IWebHostBuilder UseStartup(this IWebHostBuilder hostBuilder, Type startupType)
        {
            if (startupType == null)
            {
                throw new ArgumentNullException(nameof(startupType));
            }

            var startupAssemblyName = startupType.Assembly.GetName().Name;

            hostBuilder.UseSetting(WebHostDefaults.ApplicationKey, startupAssemblyName);

            // Light up the GenericWebHostBuilder implementation
            if (hostBuilder is ISupportsStartup supportsStartup)
            {
                return supportsStartup.UseStartup(startupType);
            }

            return hostBuilder
                .ConfigureServices(services =>
                {
                    if (typeof(IStartup).IsAssignableFrom(startupType))
                    {
                        services.AddSingleton(typeof(IStartup), startupType);
                    }
                    else
                    {
                        services.AddSingleton(typeof(IStartup), sp =>
                        {
                            var hostingEnvironment = sp.GetRequiredService<IWebHostEnvironment>();
                            return new ConventionBasedStartup(StartupLoader.LoadMethods(sp, startupType, hostingEnvironment.EnvironmentName));
                        });
                    }
                });
        }

        /// <summary>
        /// Specify the startup type to be used by the web host.
        /// </summary>
        /// <param name="hostBuilder">The <see cref="IWebHostBuilder"/> to configure.</param>
        /// <typeparam name ="TStartup">The type containing the startup methods for the application.</typeparam>
        /// <returns>The <see cref="IWebHostBuilder"/>.</returns>
        public static IWebHostBuilder UseStartup<TStartup>(this IWebHostBuilder hostBuilder) where TStartup : class
        {
            return hostBuilder.UseStartup(typeof(TStartup));
        }

        /// <summary>
        /// Configures the default service provider
        /// </summary>
        /// <param name="hostBuilder">The <see cref="IWebHostBuilder"/> to configure.</param>
        /// <param name="configure">A callback used to configure the <see cref="ServiceProviderOptions"/> for the default <see cref="IServiceProvider"/>.</param>
        /// <returns>The <see cref="IWebHostBuilder"/>.</returns>
        public static IWebHostBuilder UseDefaultServiceProvider(this IWebHostBuilder hostBuilder, Action<ServiceProviderOptions> configure)
        {
            return hostBuilder.UseDefaultServiceProvider((context, options) => configure(options));
        }

        /// <summary>
        /// Configures the default service provider
        /// </summary>
        /// <param name="hostBuilder">The <see cref="IWebHostBuilder"/> to configure.</param>
        /// <param name="configure">A callback used to configure the <see cref="ServiceProviderOptions"/> for the default <see cref="IServiceProvider"/>.</param>
        /// <returns>The <see cref="IWebHostBuilder"/>.</returns>
        public static IWebHostBuilder UseDefaultServiceProvider(this IWebHostBuilder hostBuilder, Action<WebHostBuilderContext, ServiceProviderOptions> configure)
        {
            // Light up the GenericWebHostBuilder implementation
            if (hostBuilder is ISupportsUseDefaultServiceProvider supportsDefaultServiceProvider)
            {
                return supportsDefaultServiceProvider.UseDefaultServiceProvider(configure);
            }

            return hostBuilder.ConfigureServices((context, services) =>
            {
                var options = new ServiceProviderOptions();
                configure(context, options);
                services.Replace(ServiceDescriptor.Singleton<IServiceProviderFactory<IServiceCollection>>(new SystemWebServiceProviderFactory(options)));
            });
        }

        /// <summary>
        /// Adds a delegate for configuring the <see cref="IConfigurationBuilder"/> that will construct an <see cref="IConfiguration"/>.
        /// </summary>
        /// <param name="hostBuilder">The <see cref="IWebHostBuilder"/> to configure.</param>
        /// <param name="configureDelegate">The delegate for configuring the <see cref="IConfigurationBuilder" /> that will be used to construct an <see cref="IConfiguration" />.</param>
        /// <returns>The <see cref="IWebHostBuilder"/>.</returns>
        /// <remarks>
        /// The <see cref="IConfiguration"/> and <see cref="ILoggerFactory"/> on the <see cref="WebHostBuilderContext"/> are uninitialized at this stage.
        /// The <see cref="IConfigurationBuilder"/> is pre-populated with the settings of the <see cref="IWebHostBuilder"/>.
        /// </remarks>
        public static IWebHostBuilder ConfigureAppConfiguration(this IWebHostBuilder hostBuilder, Action<IConfigurationBuilder> configureDelegate)
        {
            return hostBuilder.ConfigureAppConfiguration((context, builder) => configureDelegate(builder));
        }

        /// <summary>
        /// Adds a delegate for configuring the provided <see cref="ILoggingBuilder"/>. This may be called multiple times.
        /// </summary>
        /// <param name="hostBuilder">The <see cref="IWebHostBuilder" /> to configure.</param>
        /// <param name="configureLogging">The delegate that configures the <see cref="ILoggingBuilder"/>.</param>
        /// <returns>The <see cref="IWebHostBuilder"/>.</returns>
        public static IWebHostBuilder ConfigureLogging(this IWebHostBuilder hostBuilder, Action<ILoggingBuilder> configureLogging)
        {
            return hostBuilder.ConfigureServices(collection => collection.AddLogging(configureLogging));
        }

        /// <summary>
        /// Adds a delegate for configuring the provided <see cref="LoggerFactory"/>. This may be called multiple times.
        /// </summary>
        /// <param name="hostBuilder">The <see cref="IWebHostBuilder" /> to configure.</param>
        /// <param name="configureLogging">The delegate that configures the <see cref="LoggerFactory"/>.</param>
        /// <returns>The <see cref="IWebHostBuilder"/>.</returns>
        public static IWebHostBuilder ConfigureLogging(this IWebHostBuilder hostBuilder, Action<WebHostBuilderContext, ILoggingBuilder> configureLogging)
        {
            return hostBuilder.ConfigureServices((context, collection) => collection.AddLogging(builder => configureLogging(context, builder)));
        }
    }
}
