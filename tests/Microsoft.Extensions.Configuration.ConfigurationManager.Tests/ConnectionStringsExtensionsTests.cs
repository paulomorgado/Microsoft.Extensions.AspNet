using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Microsoft.Extensions.Configuration.ConfigurationManager.Tests
{
    public static class ConnectionStringsExtensionsTests
    {
        [Fact]
        public static void ToKeyValuePairEnumerable_WithNullCollection_ThrowsArgumentNullException()
        {
            Should.Throw<ArgumentNullException>(() => ConnectionStringsExtensions.ToKeyValuePairEnumerable(null));
        }

        public static TheoryData<ConnectionStringSettingsCollection, IEnumerable<KeyValuePair<string, string>>> TestData = new TheoryData<ConnectionStringSettingsCollection, IEnumerable<KeyValuePair<string, string>>>
        {
            {
                new  ConnectionStringSettingsCollection(),
                Enumerable.Empty<KeyValuePair<string, string>>()
            },
            {
                new  ConnectionStringSettingsCollection
                {
                    new ConnectionStringSettings("conectionName", null)
                },
                new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("ConnectionStrings:conectionName",null),
                }
            },
            {
                new  ConnectionStringSettingsCollection
                {
                    new ConnectionStringSettings("conectionName", "connection string")
                },
                new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("ConnectionStrings:conectionName","connection string"),
                }
            },
            {
                new  ConnectionStringSettingsCollection
                {
                    new ConnectionStringSettings("conectionName", "connection string", "providerName")
                },
                new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("ConnectionStrings:conectionName","connection string"),
                    new KeyValuePair<string, string>("ConnectionStrings:conectionName_ProviderName","providerName"),
                }
            },
        };

        [Theory]
        [MemberData(nameof(TestData))]
        public static void ToKeyValuePairEnumerable_Tests(ConnectionStringSettingsCollection collection, IEnumerable<KeyValuePair<string, string>> expectedResult)
        {
            // Act

            var actual = collection.ToKeyValuePairEnumerable();

            // Assert

            actual.ShouldBe(expectedResult);
        }
    }
}
