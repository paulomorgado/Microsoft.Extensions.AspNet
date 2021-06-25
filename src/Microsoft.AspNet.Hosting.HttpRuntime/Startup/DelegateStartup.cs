using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.HttpRuntime.Hosting;

namespace Microsoft.AspNet.Hosting.HttpRuntime.Startup
{
    /// <summary>
    /// Used for initializing services and middlewares used by an application.
    /// </summary>
    public class DelegateStartup : StartupBase<IServiceCollection>
    {
        private Action<IHttpRuntimeApplicationBuilder> _configureApp;

        /// <summary>
        /// Creates a new <see cref="DelegateStartup" /> instance.
        /// </summary>
        /// <param name="factory">A factory for creating <see cref="IServiceProvider"/> instances.</param>
        /// <param name="configureApp">An <see cref="Action"/> for configuring the application.</param>
        public DelegateStartup(IServiceProviderFactory<IServiceCollection> factory, Action<IHttpRuntimeApplicationBuilder> configureApp) : base(factory)
        {
            _configureApp = configureApp;
        }

        /// <summary>
        /// Configures the <see cref="IApplicationBuilder"/> with the initialized <see cref="Action"/>.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/>.</param>
        public override void Configure(IHttpRuntimeApplicationBuilder app) => _configureApp(app);
    }
}
