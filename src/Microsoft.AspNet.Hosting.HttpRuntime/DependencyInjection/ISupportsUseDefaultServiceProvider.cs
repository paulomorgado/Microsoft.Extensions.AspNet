using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.Hosting.HttpRuntime.DependencyInjection
{
    internal interface ISupportsUseDefaultServiceProvider
    {
        IHttpRuntimeWebHostBuilder UseDefaultServiceProvider(Action<HttpRuntimeWebHostBuilderContext, ServiceProviderOptions> configure);
    }
}
