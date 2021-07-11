using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Shouldly;
using Xunit;

namespace Microsoft.Extensions.Configuration.ConfigurationManager.Tests
{
    public static class NameValueCollectionExtensionsTests
    {
        private static readonly NameValueCollection sampleNameValueCollection = new NameValueCollection
            {
               {  "area0:key1", "area0_key1_value" },
               {  "area0:key2", "area0_key2_value" },
               {  "area0:key3", "area0_key3_value" },
               {  "area1:key1", "area1_key1_value1" },
               {  "area1:key1", "area1_key1_value2" },
               {  "area1:key1", "area1_key1_value3" },
               {  "area1:key2", "area1_key2_value1" },
               {  "area1:key2", "area1_key2_value2" },
               {  "area1:key2", "area1_key2_value3" },
               {  "area2:key3", "area2_key3_value1" },
               {  "area2:key3", "area2_key3_value2" },
               {  "area2:key3", "area2_key3_value3" },
            };

        private static readonly KeyValuePair<string, string>[] expectedFlatValues = new KeyValuePair<string, string>[]
            {
               new KeyValuePair<string, string>(  "area0:key1", "area0_key1_value"),
               new KeyValuePair<string, string>(  "area0:key2", "area0_key2_value"),
               new KeyValuePair<string, string>(  "area0:key3", "area0_key3_value"),
               new KeyValuePair<string, string>(  "area1:key1", "area1_key1_value1,area1_key1_value2,area1_key1_value3"),
               new KeyValuePair<string, string>(  "area1:key2", "area1_key2_value1,area1_key2_value2,area1_key2_value3"),
               new KeyValuePair<string, string>(  "area2:key3", "area2_key3_value1,area2_key3_value2,area2_key3_value3"),
            };

        private static readonly KeyValuePair<string, string>[] expectedStrictValues = new KeyValuePair<string, string>[]
            {
               new KeyValuePair<string, string>(  "area0:key1:0", "area0_key1_value"),
               new KeyValuePair<string, string>(  "area0:key2:0", "area0_key2_value"),
               new KeyValuePair<string, string>(  "area0:key3:0", "area0_key3_value"),
               new KeyValuePair<string, string>(  "area1:key1:0", "area1_key1_value1"),
               new KeyValuePair<string, string>(  "area1:key1:1", "area1_key1_value2"),
               new KeyValuePair<string, string>(  "area1:key1:2", "area1_key1_value3"),
               new KeyValuePair<string, string>(  "area1:key2:0", "area1_key2_value1"),
               new KeyValuePair<string, string>(  "area1:key2:1", "area1_key2_value2"),
               new KeyValuePair<string, string>(  "area1:key2:2", "area1_key2_value3"),
               new KeyValuePair<string, string>(  "area2:key3:0", "area2_key3_value1"),
               new KeyValuePair<string, string>(  "area2:key3:1", "area2_key3_value2"),
               new KeyValuePair<string, string>(  "area2:key3:2", "area2_key3_value3"),
            };

        private static readonly KeyValuePair<string, string>[] expectedMixedValues = new KeyValuePair<string, string>[]
            {
               new KeyValuePair<string, string>(  "area0:key1", "area0_key1_value"),
               new KeyValuePair<string, string>(  "area0:key2", "area0_key2_value"),
               new KeyValuePair<string, string>(  "area0:key3", "area0_key3_value"),
               new KeyValuePair<string, string>(  "area1:key1:0", "area1_key1_value1"),
               new KeyValuePair<string, string>(  "area1:key1:1", "area1_key1_value2"),
               new KeyValuePair<string, string>(  "area1:key1:2", "area1_key1_value3"),
               new KeyValuePair<string, string>(  "area1:key2:0", "area1_key2_value1"),
               new KeyValuePair<string, string>(  "area1:key2:1", "area1_key2_value2"),
               new KeyValuePair<string, string>(  "area1:key2:2", "area1_key2_value3"),
               new KeyValuePair<string, string>(  "area2:key3:0", "area2_key3_value1"),
               new KeyValuePair<string, string>(  "area2:key3:1", "area2_key3_value2"),
               new KeyValuePair<string, string>(  "area2:key3:2", "area2_key3_value3"),
            };

        public static readonly TheoryData<NameValueCollectionEnumerationMode, string, IEnumerable<KeyValuePair<string, string>>> TestData = new TheoryData<NameValueCollectionEnumerationMode, string, IEnumerable<KeyValuePair<string, string>>>
        {
            { NameValueCollectionEnumerationMode.Flat, null, expectedFlatValues },
            { NameValueCollectionEnumerationMode.Flat, string.Empty, expectedFlatValues },
            { NameValueCollectionEnumerationMode.Flat, "area0:", expectedFlatValues },
            { NameValueCollectionEnumerationMode.Flat, "area1:", expectedFlatValues },
            { NameValueCollectionEnumerationMode.Flat, "area2:", expectedFlatValues },
            { NameValueCollectionEnumerationMode.Strict, null, expectedStrictValues },
            { NameValueCollectionEnumerationMode.Strict, string.Empty, expectedStrictValues },
            { NameValueCollectionEnumerationMode.Strict, "area0:", expectedStrictValues },
            { NameValueCollectionEnumerationMode.Strict, "area1:", expectedStrictValues },
            { NameValueCollectionEnumerationMode.Strict, "area2:", expectedStrictValues },
            { NameValueCollectionEnumerationMode.Mixed, null, expectedMixedValues },
            { NameValueCollectionEnumerationMode.Mixed, string.Empty, expectedMixedValues },
            { NameValueCollectionEnumerationMode.Mixed, "area0:", expectedMixedValues },
            { NameValueCollectionEnumerationMode.Mixed, "area1:", expectedMixedValues },
            { NameValueCollectionEnumerationMode.Mixed, "area2:", expectedMixedValues },
        };

        public static readonly TheoryData<string, IEnumerable<KeyValuePair<string, string>>> FlatTestData = GetTestData(NameValueCollectionEnumerationMode.Flat);

        public static readonly TheoryData<string, IEnumerable<KeyValuePair<string, string>>> StrictTestData = GetTestData(NameValueCollectionEnumerationMode.Strict);

        public static readonly TheoryData<string, IEnumerable<KeyValuePair<string, string>>> MixedTestData = GetTestData(NameValueCollectionEnumerationMode.Mixed);

        [Fact]
        public static void ToFlatKeyValuePairEnumerable_WithNullCollection_ThrowsArgumentNullException()
        {
            Should.Throw<ArgumentNullException>(() => NameValueCollectionExtensions.ToFlatKeyValuePairEnumerable(null));
        }

        [Theory]
        [MemberData(nameof(FlatTestData))]
        public static void ToFlatKeyValuePairEnumerable_Tests(string prefix, IEnumerable<KeyValuePair<string, string>> expectedResult)
        {
            // Act

            var actual = sampleNameValueCollection.ToFlatKeyValuePairEnumerable(prefix);

            // Assert

            actual.ShouldBe(Filter(prefix, expectedResult));
        }

        [Fact]
        public static void ToStricktKeyValuePairEnumerable_WithNullCollection_ThrowsArgumentNullException()
        {
            Should.Throw<ArgumentNullException>(() => NameValueCollectionExtensions.ToStrictKeyValuePairEnumerable(null));
        }

        [Theory]
        [MemberData(nameof(StrictTestData))]
        public static void ToStrictKeyValuePairEnumerable_Tests(string prefix, IEnumerable<KeyValuePair<string, string>> expectedResult)
        {
            // Act

            var actual = sampleNameValueCollection.ToStrictKeyValuePairEnumerable(prefix);

            // Assert

            actual.ShouldBe(Filter(prefix, expectedResult));
        }

        [Fact]
        public static void ToMixedKeyValuePairEnumerable_WithNullCollection_ThrowsArgumentNullException()
        {
            Should.Throw<ArgumentNullException>(() => NameValueCollectionExtensions.ToMixedKeyValuePairEnumerable(null));
        }

        [Theory]
        [MemberData(nameof(MixedTestData))]
        public static void ToMixedKeyValuePairEnumerable_Tests(string prefix, IEnumerable<KeyValuePair<string, string>> expectedResult)
        {
            // Act

            var actual = sampleNameValueCollection.ToMixedKeyValuePairEnumerable(prefix);

            // Assert

            actual.ShouldBe(Filter(prefix, expectedResult));
        }

        [Fact]
        public static void ToKeyValuePairEnumerable_WithNullCollection_ThrowsArgumentNullException()
        {
            Should.Throw<ArgumentNullException>(() => NameValueCollectionExtensions.ToKeyValuePairEnumerable(null, NameValueCollectionEnumerationMode.Flat));
        }

        [Fact]
        public static void ToKeyValuePairEnumerable_WithInvalidMode_ThrowsArgumentOutOfRangeException()
        {
            Should.Throw<ArgumentOutOfRangeException>(() => NameValueCollectionExtensions.ToKeyValuePairEnumerable(sampleNameValueCollection, (NameValueCollectionEnumerationMode)(-1)));
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public static void ToKeyValuePairEnumerable_Tests(NameValueCollectionEnumerationMode mode, string prefix, IEnumerable<KeyValuePair<string, string>> expectedResult)
        {
            // Act

            var actual = sampleNameValueCollection.ToKeyValuePairEnumerable(mode, prefix);

            // Assert

            actual.ShouldBe(Filter(prefix, expectedResult));
        }

        private static TheoryData<string, IEnumerable<KeyValuePair<string, string>>> GetTestData(NameValueCollectionEnumerationMode mode)
        {
            var data = new TheoryData<string, IEnumerable<KeyValuePair<string, string>>>();

            foreach (var item in TestData)
            {
                if ((NameValueCollectionEnumerationMode)item[0] == mode)
                {
                    data.Add((string)item[1], (IEnumerable<KeyValuePair<string, string>>)item[2]);
                }
            }

            return data;
        }

        private static IEnumerable<KeyValuePair<string, string>> Filter(string prefix, IEnumerable<KeyValuePair<string, string>> data)
            => data
                .Where(p => p.Key.StartsWith(prefix ?? string.Empty))
                .Select(p => new KeyValuePair<string, string>(p.Key.Substring(prefix?.Length ?? 0), p.Value));
    }
}
