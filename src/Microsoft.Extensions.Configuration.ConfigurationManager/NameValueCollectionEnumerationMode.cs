using System.Collections.Specialized;

namespace Microsoft.Extensions.Configuration.ConfigurationManager
{
    /// <summary>
    /// Defines the possible modes for enumerating configuration from a <see cref="NameValueCollection"/>.
    /// </summary>
    public enum NameValueCollectionEnumerationMode
    {
        /// <summary>
        /// All keys are treated as having a single value.
        /// </summary>
        /// <remarks>Invokes <see cref="NameValueCollectionExtensions.ToFlatKeyValuePairEnumerable(NameValueCollection, string)"/></remarks>
        Flat,

        /// <summary>
        /// All keys are treated as having a list of values.
        /// </summary>
        /// <remarks>Invokes <see cref="NameValueCollectionExtensions.ToStrictKeyValuePairEnumerable(NameValueCollection, string)"/></remarks>
        Strict,

        /// <summary>
        /// All keys are treated as having a single value, if there's only one value or a list of values if there's more than one.
        /// </summary>
        /// <remarks>Invokes <see cref="NameValueCollectionExtensions.ToMixedKeyValuePairEnumerable(NameValueCollection, string)"/></remarks>
        Mixed,
    }
}
