using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.Hosting.HttpRuntime.DependencyInjection
{
    internal interface ISupportsUseDefaultServiceProvider
    {
        IWebHostBuilder UseDefaultServiceProvider(Action<WebHostBuilderContext, ServiceProviderOptions> configure);
    }
}
