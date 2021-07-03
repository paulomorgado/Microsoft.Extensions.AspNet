namespace Microsoft.AspNet.Hosting
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
        /// Indicates if "Microsoft:AspNet:Hosting:HttpRuntime:" prefixed <see cref="WebConfigurationManager.AppSettings"/> should be added to configuration.
        /// They are added by default.
        /// </summary>
        public bool SuppressAppSettingsConfiguration { get; set; }
    }
}
