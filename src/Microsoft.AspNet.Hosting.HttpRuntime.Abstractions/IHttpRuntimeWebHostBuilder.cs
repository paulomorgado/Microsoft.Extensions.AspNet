using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.Hosting
{
    /// <summary>
    /// A builder for <see cref="IHttpRuntimeWebHost"/>.
    /// </summary>
    public interface IHttpRuntimeWebHostBuilder
    {
        /// <summary>
        /// Builds an <see cref="IHttpRuntimeWebHost"/> which hosts a web application.
        /// </summary>
        IHttpRuntimeWebHost Build();

        /// <summary>
        /// Adds a delegate for configuring the <see cref="IConfigurationBuilder"/> that will construct an <see cref="IConfiguration"/>.
        /// </summary>
        /// <param name="configureDelegate">The delegate for configuring the <see cref="IConfigurationBuilder" /> that will be used to construct an <see cref="IConfiguration" />.</param>
        /// <returns>The <see cref="IHttpRuntimeWebHostBuilder"/>.</returns>
        /// <remarks>
        /// The <see cref="IConfiguration"/> and <see cref="ILoggerFactory"/> on the <see cref="HttpRuntimeWebHostBuilderContext"/> are uninitialized at this stage.
        /// The <see cref="IConfigurationBuilder"/> is pre-populated with the settings of the <see cref="IHttpRuntimeWebHostBuilder"/>.
        /// </remarks>
        IHttpRuntimeWebHostBuilder ConfigureAppConfiguration(Action<HttpRuntimeWebHostBuilderContext, IConfigurationBuilder> configureDelegate);

        /// <summary>
        /// Adds a delegate for configuring additional services for the host or web application. This may be called
        /// multiple times.
        /// </summary>
        /// <param name="configureServices">A delegate for configuring the <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IHttpRuntimeWebHostBuilder"/>.</returns>
        IHttpRuntimeWebHostBuilder ConfigureServices(Action<IServiceCollection> configureServices);

        /// <summary>
        /// Adds a delegate for configuring additional services for the host or web application. This may be called
        /// multiple times.
        /// </summary>
        /// <param name="configureServices">A delegate for configuring the <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IHttpRuntimeWebHostBuilder"/>.</returns>
        IHttpRuntimeWebHostBuilder ConfigureServices(Action<HttpRuntimeWebHostBuilderContext, IServiceCollection> configureServices);

        /// <summary>
        /// Get the setting value from the configuration.
        /// </summary>
        /// <param name="key">The key of the setting to look up.</param>
        /// <returns>The value the setting currently contains.</returns>
        string GetSetting(string key);

        /// <summary>
        /// Add or replace a setting in the configuration.
        /// </summary>
        /// <param name="key">The key of the setting to add or replace.</param>
        /// <param name="value">The value of the setting to add or replace.</param>
        /// <returns>The <see cref="IHttpRuntimeWebHostBuilder"/>.</returns>
        IHttpRuntimeWebHostBuilder UseSetting(string key, string value);
    }
}
