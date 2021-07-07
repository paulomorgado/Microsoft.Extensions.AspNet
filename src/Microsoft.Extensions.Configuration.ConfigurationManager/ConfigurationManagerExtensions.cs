using System;

namespace Microsoft.Extensions.Configuration.ConfigurationManager
{
    /// <summary>
    /// Extension methods for registering <see cref="ConfigurationManagerConfigurationProvider" /> with <see cref="IConfigurationBuilder" />.
    /// </summary>
    public static class ConfigurationManagerExtensions
    {
        /// <summary>
        /// Adds an <see cref="IConfigurationProvider" /> that reads configuration values from <see cref="System.Configuration.ConfigurationManager" />.
        /// </summary>
        /// <param name="configurationBuilder">The <see cref="IConfigurationBuilder" /> to add to.</param>
        /// <returns>The <see cref="IConfigurationBuilder" />.</returns>
        public static IConfigurationBuilder AddConfigurationManager(this IConfigurationBuilder configurationBuilder)
            => configurationBuilder.AddConfigurationManager(null, false, NameValueCollectionEnumerationMode.Flat);

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider" /> that reads configuration values from <see cref="System.Configuration.ConfigurationManager" />
        /// with a specified prefix.
        /// </summary>
        /// <param name="configurationBuilder">The <see cref="IConfigurationBuilder" /> to add to.</param>
        /// <param name="prefix">The prefix that environment variable names must start with. The prefix will be removed from the environment variable names.</param>
        /// <returns>The <see cref="IConfigurationBuilder" />.</returns>
        public static IConfigurationBuilder AddConfigurationManager(
            this IConfigurationBuilder configurationBuilder,
            string prefix)
            => configurationBuilder.AddConfigurationManager(prefix, false, NameValueCollectionEnumerationMode.Flat);

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider" /> that reads configuration values from <see cref="System.Configuration.ConfigurationManager" />
        /// with a specified prefix and indication whether to skip loading connection strings.
        /// </summary>
        /// <param name="configurationBuilder">The configuration builder.</param>
        /// <param name="prefix">The prefix.</param>
        /// <param name="skipConnectionStrings">if set to <see langword="true" /> skip loading <see cref="System.Configuration.ConfigurationManager.ConnectionStrings" />.</param>
        /// <returns>The <see cref="IConfigurationBuilder" />.</returns>
        public static IConfigurationBuilder AddConfigurationManager(
            this IConfigurationBuilder configurationBuilder,
            string prefix,
            bool skipConnectionStrings)
            => configurationBuilder.AddConfigurationManager(prefix, skipConnectionStrings, NameValueCollectionEnumerationMode.Flat);

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider" /> that reads configuration values from <see cref="System.Configuration.ConfigurationManager" />
        /// with a specified prefix, indication whether to skip loading connection strings and <see cref="System.Configuration.ConfigurationManager.AppSettings" /> <see cref="NameValueCollectionEnumerationMode">enumeration mode</see>.
        /// </summary>
        /// <param name="configurationBuilder">The configuration builder.</param>
        /// <param name="prefix">The prefix.</param>
        /// <param name="skipConnectionStrings">if set to <see langword="true" /> skip loading <see cref="System.Configuration.ConfigurationManager.ConnectionStrings" />.</param>
        /// <param name="appSettingsEnumerationMode">The application settings enumeration mode.</param>
        /// <returns>The <see cref="IConfigurationBuilder" />.</returns>
        public static IConfigurationBuilder AddConfigurationManager(
            this IConfigurationBuilder configurationBuilder,
            string prefix,
            bool skipConnectionStrings,
            NameValueCollectionEnumerationMode appSettingsEnumerationMode)
        {
            configurationBuilder.Add(new ConfigurationManagerConfigurationSource
            {
                Prefix = prefix,
                SkipConnectionStrings = skipConnectionStrings,
                AppSettingsEnumerationMode = appSettingsEnumerationMode,
            });

            return configurationBuilder;
        }

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider" /> that reads configuration values from <see cref="System.Configuration.ConfigurationManager" />.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder" /> to add to.</param>
        /// <param name="configureSource">Configures the source.</param>
        /// <returns>The <see cref="IConfigurationBuilder" />.</returns>
        public static IConfigurationBuilder AddConfigurationManager(this IConfigurationBuilder builder, Action<ConfigurationManagerConfigurationSource> configureSource)
            => builder.Add(configureSource);
    }
}
