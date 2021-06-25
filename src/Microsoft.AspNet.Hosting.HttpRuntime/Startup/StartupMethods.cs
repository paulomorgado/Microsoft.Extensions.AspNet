using System;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.HttpRuntime.Hosting;

namespace Microsoft.AspNet.Hosting.HttpRuntime.Startup
{
    internal class StartupMethods
    {
        public StartupMethods(object instance, Action<IHttpRuntimeApplicationBuilder> configure, Func<IServiceCollection, IServiceProvider> configureServices)
        {
            Debug.Assert(configure != null);
            Debug.Assert(configureServices != null);

            StartupInstance = instance;
            ConfigureDelegate = configure;
            ConfigureServicesDelegate = configureServices;
        }

        public object StartupInstance { get; }
        public Func<IServiceCollection, IServiceProvider> ConfigureServicesDelegate { get; }
        public Action<IHttpRuntimeApplicationBuilder> ConfigureDelegate { get; }

    }
}
