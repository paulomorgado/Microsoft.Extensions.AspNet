using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;

namespace Microsoft.Extensions.Configuration.ConfigurationManager
{
    /// <summary>
    /// Provides extensions to convert The <see cref="NameValueCollection"/> converted into a <see cref="IEnumerable{T}"/> of <see cref="KeyValuePair{TKey, TValue}"/>.
    /// </summary>
    public static class NameValueCollectionExtensions
    {
        /// <summary>
        /// Converts The <see cref="NameValueCollection"/> converted into a <see cref="IEnumerable{T}"/> of <see cref="KeyValuePair{TKey, TValue}"/> using flatten values.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="preffix">The preffix.</param>
        /// <returns>The <see cref="NameValueCollection"/> converted into a <see cref="IEnumerable{T}"/> of <see cref="KeyValuePair{TKey, TValue}"/> using flatten values.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="collection"/> is <see langword="null"/>.</exception>
        public static IEnumerable<KeyValuePair<string, string>> ToFlatKeyValuePairEnumerable(this NameValueCollection collection, string preffix = null)
            => ToFlatKeyValuePairEnumerableImpl(collection ?? throw new System.ArgumentNullException(nameof(collection)), preffix ?? string.Empty);

        /// <summary>
        /// Converts The <see cref="NameValueCollection"/> converted into a <see cref="IEnumerable{T}"/> of <see cref="KeyValuePair{TKey, TValue}"/> using arrays of values.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="preffix">The preffix.</param>
        /// <returns>The <see cref="NameValueCollection"/> converted into a <see cref="IEnumerable{T}"/> of <see cref="KeyValuePair{TKey, TValue}"/> using arrays of values.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="collection"/> is <see langword="null"/>.</exception>
        public static IEnumerable<KeyValuePair<string, string>> ToStrictKeyValuePairEnumerable(this NameValueCollection collection, string preffix = null)
            => ToStrictKeyValuePairEnumerableImpl(collection ?? throw new System.ArgumentNullException(nameof(collection)), preffix ?? string.Empty);

        /// <summary>
        /// Converts The <see cref="NameValueCollection"/> converted into a <see cref="IEnumerable{T}"/> of <see cref="KeyValuePair{TKey, TValue}"/> using a mix of single string values and arrays of values.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="preffix">The preffix.</param>
        /// <returns>The <see cref="NameValueCollection"/> converted into a <see cref="IEnumerable{T}"/> of <see cref="KeyValuePair{TKey, TValue}"/> using a mix of single string values and arrays of values.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="collection"/> is <see langword="null"/>.</exception>
        public static IEnumerable<KeyValuePair<string, string>> ToMixedKeyValuePairEnumerable(this NameValueCollection collection, string preffix = null)
            => ToMixedKeyValuePairEnumerableImpl(collection ?? throw new System.ArgumentNullException(nameof(collection)), preffix ?? string.Empty);

        /// <summary>
        /// Converts The <see cref="NameValueCollection"/> converted into a <see cref="IEnumerable{T}"/> of <see cref="KeyValuePair{TKey, TValue}"/> using the specified mode.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="mode">A <see langword="NameValueCollectionEnumerationMode"/> value indicating the conversion mode.</param>
        /// <param name="preffix">The preffix.</param>
        /// <returns>The <see cref="NameValueCollection"/> converted into a <see cref="IEnumerable{T}"/> of <see cref="KeyValuePair{TKey, TValue}"/> using the specified mode.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="collection"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="mode"/> has an invalid <see langword="NameValueCollectionEnumerationMode"/> value.</exception>
        public static IEnumerable<KeyValuePair<string, string>> ToKeyValuePairEnumerable(
            this NameValueCollection collection,
            NameValueCollectionEnumerationMode mode,
            string preffix = null)
        {
            if (collection is null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            switch (mode)
            {
                case NameValueCollectionEnumerationMode.Flat: return ToFlatKeyValuePairEnumerableImpl(collection, preffix ?? string.Empty);
                case NameValueCollectionEnumerationMode.Strict: return ToStrictKeyValuePairEnumerableImpl(collection, preffix ?? string.Empty);
                case NameValueCollectionEnumerationMode.Mixed: return ToMixedKeyValuePairEnumerableImpl(collection, preffix ?? string.Empty);
                default: throw new ArgumentOutOfRangeException(nameof(mode), mode, $"{nameof(mode)} must be a value of {nameof(NameValueCollectionEnumerationMode)}.");
            }
        }

        private static IEnumerable<KeyValuePair<string, string>> ToFlatKeyValuePairEnumerableImpl(NameValueCollection collection, string preffix)
        {
            Debug.Assert(!(collection is null), $"'{collection}' is null.");
            Debug.Assert(!(preffix is null), $"'{preffix}' is null.");

            for (var i = 0; i < collection.Count; i++)
            {
                var key = collection.GetKey(i);
                if (key?.StartsWith(preffix) ?? false)
                {
                    yield return new KeyValuePair<string, string>(key.Substring(preffix.Length), collection.Get(i));
                }
            }
        }

        private static IEnumerable<KeyValuePair<string, string>> ToStrictKeyValuePairEnumerableImpl(NameValueCollection collection, string preffix)
        {
            Debug.Assert(!(collection is null), $"'{collection}' is null.");
            Debug.Assert(!(preffix is null), $"'{preffix}' is null.");

            var builder = new StringBuilder();

            for (var i = 0; i < collection.Count; i++)
            {
                var key = collection.GetKey(i);
                if ((key?.StartsWith(preffix) ?? false)
                    && collection.GetValues(i) is string[] values && values.Length > 0)
                {
                    builder
                        .Clear()
                        .Append(key, preffix.Length, key.Length - preffix.Length)
                        .Append(ConfigurationPath.KeyDelimiter);
                    var baseKeyLength = builder.Length;

                    for (var v = 0; v < values.Length; v++)
                    {
                        builder.Length = baseKeyLength;
                        builder.Append(v);

                        yield return new KeyValuePair<string, string>(builder.ToString(), values[v]);
                    }
                }
            }
        }

        private static IEnumerable<KeyValuePair<string, string>> ToMixedKeyValuePairEnumerableImpl(NameValueCollection collection, string preffix)
        {
            Debug.Assert(!(collection is null), $"'{collection}' is null.");
            Debug.Assert(!(preffix is null), $"'{preffix}' is null.");

            var builder = new StringBuilder();

            for (var i = 0; i < collection.Count; i++)
            {
                var key = collection.GetKey(i);
                if ((key?.StartsWith(preffix) ?? false)
                        && collection.GetValues(i) is string[] values && values.Length > 0)
                {
                    if (values.Length == 1)
                    {
                        yield return new KeyValuePair<string, string>(key.Substring(preffix.Length), collection.Get(i));
                    }
                    else
                    {
                        builder
                            .Clear()
                            .Append(key, preffix.Length, key.Length - preffix.Length)
                            .Append(ConfigurationPath.KeyDelimiter);
                        var baseKeyLength = builder.Length;

                        for (var v = 0; v < values.Length; v++)
                        {
                            builder.Length = baseKeyLength;
                            builder.Append(v);

                            yield return new KeyValuePair<string, string>(builder.ToString(), values[v]);
                        }
                    }
                }
            }
        }
    }
}
