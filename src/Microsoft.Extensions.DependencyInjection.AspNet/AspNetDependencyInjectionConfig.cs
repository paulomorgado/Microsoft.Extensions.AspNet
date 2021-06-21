using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.AspNet;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.Extensions.DependencyInjection.AspNet
{
    /// <summary>
    /// Provides methods to add <see cref="IServiceCollection"/>-based dependency injection to a web application.
    /// </summary>
    /// <seealso cref="HttpRuntime.WebObjectActivator"/>
    public static class AspNetDependencyInjectionConfig
    {
        internal static readonly object HttpContextServiceProviderKey = new object();
        internal static readonly object HttpContextServiceScopeKey = new object();

        /// <summary>
        /// Adds <see cref="IServiceCollection"/>-based dependency injection.
        /// </summary>
        /// <param name="configureServices"></param>
        public static IServiceProvider RegisterServiceProvider(Action<IServiceCollection> configureServices = null)
        {
            var services = new ServiceCollection();

            services.AddSingleton<IHostEnvironment>(AspNetHostEnvironment.Instance);

            configureServices?.Invoke(services);

            services.AddConfiguration();

            services.AddLogging(builder =>
            {
                builder.AddDebug();
            });

            var serviceProvider = services.BuildServiceProvider();

            var aspNetRootServiceProvider = new AspNetRootServiceProvider(serviceProvider);
            var aspNetServiceProvider = new AspNetHttpContextServiceProvider(aspNetRootServiceProvider, serviceProvider);

            HttpRuntime.WebObjectActivator = aspNetServiceProvider;

            return aspNetServiceProvider;
        }

        /// <summary>
        /// Registers begin and end request event handling for a <see cref="HttpApplication"/> instance.
        /// </summary>
        /// <param name="httpApplication">A <see cref="HttpApplication"/> instance to register begin and end request event handling.</param>
        public static void RegisterHttpApplication(HttpApplication httpApplication)
        {
            if (httpApplication is null)
            {
                throw new ArgumentNullException(nameof(httpApplication));
            }

            httpApplication.BeginRequest += (sender, e) =>
            {
                (sender as HttpApplication).Context.Items[HttpContextServiceProviderKey] = HttpRuntime.WebObjectActivator;
            };

            httpApplication.EndRequest += (sender, e) =>
            {
                if ((sender as HttpApplication).Context.Items[HttpContextServiceScopeKey] is IServiceScope serviceScope)
                {
                    serviceScope.Dispose();
                }
            };
        }
    }
}
