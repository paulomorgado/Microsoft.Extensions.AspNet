using System.Configuration;

namespace Microsoft.AspNet.Hosting.SystemWeb
{
    /// <summary>
    /// Builder options for use with ConfigureWebHost.
    /// </summary>
    public class GenericWebHostBuilderOptions
    {
        /// <summary>
        /// Indicates if "ASPNET_" prefixed environment variables should be added to configuration.
        /// They are added by default.
        /// </summary>
        public bool SuppressEnvironmentConfiguration { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether "aspnet:" prefixed <see cref="ConfigurationManager.AppSettings"/>.
        /// </summary>
        /// <value><see langword="true" /> if [suppress configuration configuration]; otherwise, <see langword="false" />.</value>
        public bool SuppressConfigurationConfiguration { get;  set; }
    }
}
