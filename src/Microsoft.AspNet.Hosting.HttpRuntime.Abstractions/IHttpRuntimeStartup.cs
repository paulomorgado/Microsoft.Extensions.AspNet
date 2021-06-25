using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.HttpRuntime.Hosting;

namespace Microsoft.AspNet.Hosting
{
    /// <summary>
    /// Provides an interface for initializing services and middleware used by an application.
    /// </summary>
    public interface IHttpRuntimeStartup
    {
        /// <summary>
        /// Register services into the <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        IServiceProvider ConfigureServices(IServiceCollection services);

        /// <summary>
        /// Configures the application.
        /// </summary>
        /// <param name="app">An <see cref="IApplicationBuilder"/> for the app to configure.</param>
        void Configure(IHttpRuntimeApplicationBuilder app);
    }
}
