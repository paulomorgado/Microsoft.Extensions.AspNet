using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Text;

namespace Microsoft.Extensions.Configuration.ConfigurationManager
{
    /// <summary>
    /// Provides extensions to convert a <see cref="ConnectionStringSettingsCollection"/> into a <see cref="IEnumerable{T}"/> of <see cref="KeyValuePair{TKey, TValue}"/>.
    /// </summary>
    public static class ConnectionStringsExtensions
    {
        public static IEnumerable<KeyValuePair<string, string>> ToKeyValuePairEnumerable(this ConnectionStringSettingsCollection connectionStrings)
            => ToKeyValuePairEnumerableImpl(connectionStrings ?? throw new ArgumentNullException(nameof(connectionStrings)));

        /// <summary>
        /// Converts a <see cref="ConnectionStringSettingsCollection"/> into a <see cref="IEnumerable{T}"/> of <see cref="KeyValuePair{TKey, TValue}"/> using flatten values.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <returns>The <see cref="ConnectionStringSettingsCollection"/> converted into a <see cref="IEnumerable{T}"/> of <see cref="KeyValuePair{TKey, TValue}"/> using flatten values.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="collection"/> is <see langword="null"/>.</exception>
        private static IEnumerable<KeyValuePair<string, string>> ToKeyValuePairEnumerableImpl(System.Configuration.ConnectionStringSettingsCollection connectionStrings)
        {
            Debug.Assert(!(connectionStrings is null), $"'{connectionStrings}' is null.");

            var builder = new StringBuilder("ConnectionStrings").Append(ConfigurationPath.KeyDelimiter);
            var baseKeyLength = builder.Length;

            foreach (ConnectionStringSettings connectionString in connectionStrings)
            {
                if (string.IsNullOrEmpty(connectionString.Name))
                {
                    continue;
                }

                builder.Length = baseKeyLength;
                builder.Append(connectionString.Name);

                yield return new KeyValuePair<string, string>(builder.ToString(), connectionString.ConnectionString);

                if (!string.IsNullOrEmpty(connectionString.ProviderName))
                {
                    builder.Append("_ProviderName");

                    yield return new KeyValuePair<string, string>(builder.ToString(), connectionString.ProviderName);
                }
            }
        }
    }
}
