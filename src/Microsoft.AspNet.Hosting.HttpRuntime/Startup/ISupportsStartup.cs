using System;
using Microsoft.AspNet.Hosting;

namespace Microsoft.HttpRuntime.Hosting
{
    internal interface ISupportsStartup
    {
        IHttpRuntimeWebHostBuilder Configure(Action<HttpRuntimeWebHostBuilderContext, IHttpRuntimeApplicationBuilder> configure);
        IHttpRuntimeWebHostBuilder UseStartup(Type startupType);
        IHttpRuntimeWebHostBuilder UseStartup<TStartup>(Func<HttpRuntimeWebHostBuilderContext, TStartup> startupFactory);
    }
}
