using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.Hosting.HttpRuntime.Startup
{
    internal class StartupMethods
    {
        public StartupMethods(object instance, Action<IApplicationBuilder> configure, Func<IServiceCollection, IServiceProvider> configureServices)
        {
            Debug.Assert(configure != null);
            Debug.Assert(configureServices != null);

            StartupInstance = instance;
            ConfigureDelegate = configure;
            ConfigureServicesDelegate = configureServices;
        }

        public object StartupInstance { get; }
        public Func<IServiceCollection, IServiceProvider> ConfigureServicesDelegate { get; }
        public Action<IApplicationBuilder> ConfigureDelegate { get; }

    }
}
