using System.Collections.Generic;
using System;
using System.Configuration;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.Configuration.ConfigurationManager
{
    /// <summary>
    /// A <see cref="System.Configuration.ConfigurationManager" /> based <see cref="ConfigurationProvider" />.
    /// </summary>
    public class ConfigurationManagerConfigurationProvider : ConfigurationProvider
    {
        //private readonly System.Configuration.ConfigurationManager configurationManager;
        /// <summary>
        /// The prefix
        /// </summary>
        private string prefix;

        /// <summary>
        /// The skip connection strings
        /// </summary>
        private bool skipConnectionStrings;

        /// <summary>
        /// The application settings enumeration mode
        /// </summary>
        private NameValueCollectionEnumerationMode appSettingsEnumerationMode;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationManagerConfigurationProvider" /> class.
        /// </summary>
        /// <param name="prefix">The prefix.</param>
        /// <param name="skipConnectionStrings">if set to <see langword="true" /> skip loading <see cref="System.Configuration.ConfigurationManager.ConnectionStrings" />.</param>
        /// <param name="appSettingsEnumerationMode">The application settings enumeration mode.</param>
        public ConfigurationManagerConfigurationProvider(string prefix, bool skipConnectionStrings, NameValueCollectionEnumerationMode appSettingsEnumerationMode)
        {
            this.prefix = prefix;
            this.skipConnectionStrings = skipConnectionStrings;
            this.appSettingsEnumerationMode = appSettingsEnumerationMode;
        }

        /// <inheritdoc/>
        public override void Load()
        {
            var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var pair in System.Configuration.ConfigurationManager.AppSettings.ToKeyValuePairEnumerable(this.appSettingsEnumerationMode, this.prefix))
            {
                data.Add(pair.Key, pair.Value);
            }

            if (!this.skipConnectionStrings)
            {
                foreach (var pair in System.Configuration.ConfigurationManager.ConnectionStrings.ToKeyValuePairEnumerable())
                {
                    data.Add(pair.Key, pair.Value);
                }
            }

            this.Data = data;
        }
    }
}
