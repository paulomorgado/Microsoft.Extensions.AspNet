using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AspNet;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AspNetConfigurationExtensions
    {
        public static IServiceCollection AddConfiguration(this IServiceCollection serviceCollection, Action<IConfigurationBuilder> configure = null)
        {
            serviceCollection.TryAddSingleton<IConfiguration>(_ => AspNetConfigurationConfig.Build(configure));

            return serviceCollection;
        }
    }
}
