namespace Microsoft.Extensions.Configuration.ConfigurationManager
{
    /// <summary>
    /// Represents a <see cref="ConfigurationManager"/> as an <see cref="IConfigurationSource"/>.
    /// </summary>
    public class ConfigurationManagerConfigurationSource : IConfigurationSource
    {
        /// <summary>
        /// A prefix used to filter <see cref="System.Configuration.ConfigurationManager.AppSettings" />.
        /// </summary>
        /// <value>The prefix.</value>
        public string Prefix { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to skip loading <see cref="System.Configuration.ConfigurationManager.ConnectionStrings" />.
        /// </summary>
        /// <value><see langword="true" /> if skip loading <see cref="System.Configuration.ConfigurationManager.ConnectionStrings" />; otherwise, <see langword="false" />.</value>
        public bool SkipConnectionStrings { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="System.Configuration.ConfigurationManager.AppSettings" /> <see cref="NameValueCollectionEnumerationMode">enumeration mode</see>.
        /// </summary>
        /// <value>The <see cref="System.Configuration.ConfigurationManager.AppSettings" /> <see cref="NameValueCollectionEnumerationMode">enumeration mode</see>.</value>
        public NameValueCollectionEnumerationMode AppSettingsEnumerationMode { get; set; }

        /// <summary>
        /// Builds the <see cref="ConfigurationManagerConfigurationProvider"/> for this source.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
        /// <returns>The <see cref="ConfigurationManagerConfigurationProvider"/> built for this source.</returns>
        public IConfigurationProvider Build(IConfigurationBuilder builder)
            => new ConfigurationManagerConfigurationProvider(this.Prefix, this.SkipConnectionStrings, this.AppSettingsEnumerationMode);
    }
}
