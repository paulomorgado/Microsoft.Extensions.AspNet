using System;
using System.Reflection;
using Microsoft.AspNet.Hosting.HttpRuntime;
using Microsoft.AspNet.Hosting.HttpRuntime.DependencyInjection;
using Microsoft.AspNet.Hosting.HttpRuntime.Startup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.HttpRuntime.Hosting;

namespace Microsoft.AspNet.Hosting
{
    /// <summary>
    /// Contains extensions for configuring an <see cref="IHttpRuntimeWebHostBuilder" />.
    /// </summary>
    public static class HttpRuntimeWebHostBuilderExtensions
    {
        /// <summary>
        /// Specify the startup method to be used to configure the web application.
        /// </summary>
        /// <param name="hostBuilder">The <see cref="IHttpRuntimeWebHostBuilder"/> to configure.</param>
        /// <param name="configureApp">The delegate that configures the <see cref="IHttpRuntimeApplicationBuilder"/>.</param>
        /// <returns>The <see cref="IHttpRuntimeWebHostBuilder"/>.</returns>
        public static IHttpRuntimeWebHostBuilder Configure(this IHttpRuntimeWebHostBuilder hostBuilder, Action<IHttpRuntimeApplicationBuilder> configureApp)
        {
            return hostBuilder.Configure((_, app) => configureApp(app), configureApp.GetMethodInfo().DeclaringType.Assembly.GetName().Name);
        }

        /// <summary>
        /// Specify the startup method to be used to configure the web application.
        /// </summary>
        /// <param name="hostBuilder">The <see cref="IHttpRuntimeWebHostBuilder"/> to configure.</param>
        /// <param name="configureApp">The delegate that configures the <see cref="IHttpRuntimeApplicationBuilder"/>.</param>
        /// <returns>The <see cref="IHttpRuntimeWebHostBuilder"/>.</returns>
        public static IHttpRuntimeWebHostBuilder Configure(this IHttpRuntimeWebHostBuilder hostBuilder, Action<HttpRuntimeWebHostBuilderContext, IHttpRuntimeApplicationBuilder> configureApp)
        {
            return hostBuilder.Configure(configureApp, configureApp.GetMethodInfo().DeclaringType.Assembly.GetName().Name);
        }

        private static IHttpRuntimeWebHostBuilder Configure(this IHttpRuntimeWebHostBuilder hostBuilder, Action<HttpRuntimeWebHostBuilderContext, IHttpRuntimeApplicationBuilder> configureApp, string startupAssemblyName)
        {
            if (configureApp == null)
            {
                throw new ArgumentNullException(nameof(configureApp));
            }

            hostBuilder.UseSetting(HttpRuntimeWebHostDefaults.ApplicationKey, startupAssemblyName);

            // Light up the ISupportsStartup implementation
            if (hostBuilder is ISupportsStartup supportsStartup)
            {
                return supportsStartup.Configure(configureApp);
            }

            return hostBuilder.ConfigureServices((context, services) =>
            {
                services.AddSingleton<IHttpRuntimeStartup>(sp =>
                {
                    return new DelegateStartup(sp.GetRequiredService<IServiceProviderFactory<IServiceCollection>>(), (app => configureApp(context, app)));
                });
            });
        }

        /// <summary>
        /// Specify a factory that creates the startup instance to be used by the web host.
        /// </summary>
        /// <param name="hostBuilder">The <see cref="IHttpRuntimeWebHostBuilder"/> to configure.</param>
        /// <param name="startupFactory">A delegate that specifies a factory for the startup class.</param>
        /// <returns>The <see cref="IHttpRuntimeWebHostBuilder"/>.</returns>
        /// <remarks>When using the il linker, all public methods of <typeparamref name="TStartup"/> are preserved. This should match the Startup type directly (and not a base type).</remarks>
        public static IHttpRuntimeWebHostBuilder UseStartup<TStartup>(this IHttpRuntimeWebHostBuilder hostBuilder, Func<HttpRuntimeWebHostBuilderContext, TStartup> startupFactory) where TStartup : class
        {
            if (startupFactory == null)
            {
                throw new ArgumentNullException(nameof(startupFactory));
            }

            var startupAssemblyName = startupFactory.GetMethodInfo().DeclaringType.Assembly.GetName().Name;

            hostBuilder.UseSetting(HttpRuntimeWebHostDefaults.ApplicationKey, startupAssemblyName);

            // Light up the GenericWebHostBuilder implementation
            if (hostBuilder is ISupportsStartup supportsStartup)
            {
                return supportsStartup.UseStartup(startupFactory);
            }

            return hostBuilder
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton(typeof(IHttpRuntimeStartup), sp =>
                    {
                        var instance = startupFactory(context) ?? throw new InvalidOperationException("The specified factory returned null startup instance.");

                        var hostingEnvironment = sp.GetRequiredService<IHttpRuntimeWebHostEnvironment>();

                        // Check if the instance implements IHttpRuntimeStartup before wrapping
                        if (instance is IHttpRuntimeStartup startup)
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
        /// <param name="hostBuilder">The <see cref="IHttpRuntimeWebHostBuilder"/> to configure.</param>
        /// <param name="startupType">The <see cref="Type"/> to be used.</param>
        /// <returns>The <see cref="IHttpRuntimeWebHostBuilder"/>.</returns>
        public static IHttpRuntimeWebHostBuilder UseStartup(this IHttpRuntimeWebHostBuilder hostBuilder, Type startupType)
        {
            if (startupType == null)
            {
                throw new ArgumentNullException(nameof(startupType));
            }

            var startupAssemblyName = startupType.Assembly.GetName().Name;

            hostBuilder.UseSetting(HttpRuntimeWebHostDefaults.ApplicationKey, startupAssemblyName);

            // Light up the GenericWebHostBuilder implementation
            if (hostBuilder is ISupportsStartup supportsStartup)
            {
                return supportsStartup.UseStartup(startupType);
            }

            return hostBuilder
                .ConfigureServices(services =>
                {
                    if (typeof(IHttpRuntimeStartup).IsAssignableFrom(startupType))
                    {
                        services.AddSingleton(typeof(IHttpRuntimeStartup), startupType);
                    }
                    else
                    {
                        services.AddSingleton(typeof(IHttpRuntimeStartup), sp =>
                        {
                            var hostingEnvironment = sp.GetRequiredService<IHttpRuntimeWebHostEnvironment>();
                            return new ConventionBasedStartup(StartupLoader.LoadMethods(sp, startupType, hostingEnvironment.EnvironmentName));
                        });
                    }
                });
        }

        /// <summary>
        /// Specify the startup type to be used by the web host.
        /// </summary>
        /// <param name="hostBuilder">The <see cref="IHttpRuntimeWebHostBuilder"/> to configure.</param>
        /// <typeparam name ="TStartup">The type containing the startup methods for the application.</typeparam>
        /// <returns>The <see cref="IHttpRuntimeWebHostBuilder"/>.</returns>
        public static IHttpRuntimeWebHostBuilder UseStartup<TStartup>(this IHttpRuntimeWebHostBuilder hostBuilder) where TStartup : class
        {
            return hostBuilder.UseStartup(typeof(TStartup));
        }

        /// <summary>
        /// Configures the default service provider
        /// </summary>
        /// <param name="hostBuilder">The <see cref="IHttpRuntimeWebHostBuilder"/> to configure.</param>
        /// <param name="configure">A callback used to configure the <see cref="ServiceProviderOptions"/> for the default <see cref="IServiceProvider"/>.</param>
        /// <returns>The <see cref="IHttpRuntimeWebHostBuilder"/>.</returns>
        public static IHttpRuntimeWebHostBuilder UseDefaultServiceProvider(this IHttpRuntimeWebHostBuilder hostBuilder, Action<ServiceProviderOptions> configure)
        {
            return hostBuilder.UseDefaultServiceProvider((context, options) => configure(options));
        }

        /// <summary>
        /// Configures the default service provider
        /// </summary>
        /// <param name="hostBuilder">The <see cref="IHttpRuntimeWebHostBuilder"/> to configure.</param>
        /// <param name="configure">A callback used to configure the <see cref="ServiceProviderOptions"/> for the default <see cref="IServiceProvider"/>.</param>
        /// <returns>The <see cref="IHttpRuntimeWebHostBuilder"/>.</returns>
        public static IHttpRuntimeWebHostBuilder UseDefaultServiceProvider(this IHttpRuntimeWebHostBuilder hostBuilder, Action<HttpRuntimeWebHostBuilderContext, ServiceProviderOptions> configure)
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
                services.Replace(ServiceDescriptor.Singleton<IServiceProviderFactory<IServiceCollection>>(new HttpRuntimeServiceProviderFactory(options)));
            });
        }

        /// <summary>
        /// Adds a delegate for configuring the <see cref="IConfigurationBuilder"/> that will construct an <see cref="IConfiguration"/>.
        /// </summary>
        /// <param name="hostBuilder">The <see cref="IHttpRuntimeWebHostBuilder"/> to configure.</param>
        /// <param name="configureDelegate">The delegate for configuring the <see cref="IConfigurationBuilder" /> that will be used to construct an <see cref="IConfiguration" />.</param>
        /// <returns>The <see cref="IHttpRuntimeWebHostBuilder"/>.</returns>
        /// <remarks>
        /// The <see cref="IConfiguration"/> and <see cref="ILoggerFactory"/> on the <see cref="HttpRuntimeWebHostBuilderContext"/> are uninitialized at this stage.
        /// The <see cref="IConfigurationBuilder"/> is pre-populated with the settings of the <see cref="IHttpRuntimeWebHostBuilder"/>.
        /// </remarks>
        public static IHttpRuntimeWebHostBuilder ConfigureAppConfiguration(this IHttpRuntimeWebHostBuilder hostBuilder, Action<IConfigurationBuilder> configureDelegate)
        {
            return hostBuilder.ConfigureAppConfiguration((context, builder) => configureDelegate(builder));
        }

        /// <summary>
        /// Adds a delegate for configuring the provided <see cref="ILoggingBuilder"/>. This may be called multiple times.
        /// </summary>
        /// <param name="hostBuilder">The <see cref="IHttpRuntimeWebHostBuilder" /> to configure.</param>
        /// <param name="configureLogging">The delegate that configures the <see cref="ILoggingBuilder"/>.</param>
        /// <returns>The <see cref="IHttpRuntimeWebHostBuilder"/>.</returns>
        public static IHttpRuntimeWebHostBuilder ConfigureLogging(this IHttpRuntimeWebHostBuilder hostBuilder, Action<ILoggingBuilder> configureLogging)
        {
            return hostBuilder.ConfigureServices(collection => collection.AddLogging(configureLogging));
        }

        /// <summary>
        /// Adds a delegate for configuring the provided <see cref="LoggerFactory"/>. This may be called multiple times.
        /// </summary>
        /// <param name="hostBuilder">The <see cref="IHttpRuntimeWebHostBuilder" /> to configure.</param>
        /// <param name="configureLogging">The delegate that configures the <see cref="LoggerFactory"/>.</param>
        /// <returns>The <see cref="IHttpRuntimeWebHostBuilder"/>.</returns>
        public static IHttpRuntimeWebHostBuilder ConfigureLogging(this IHttpRuntimeWebHostBuilder hostBuilder, Action<HttpRuntimeWebHostBuilderContext, ILoggingBuilder> configureLogging)
        {
            return hostBuilder.ConfigureServices((context, collection) => collection.AddLogging(builder => configureLogging(context, builder)));
        }
    }
}
