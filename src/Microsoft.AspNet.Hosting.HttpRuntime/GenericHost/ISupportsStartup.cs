using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Microsoft.HttpRuntime.Hosting
{
    internal interface ISupportsStartup
    {
        IWebHostBuilder Configure(Action<WebHostBuilderContext, IApplicationBuilder> configure);
        IWebHostBuilder UseStartup(Type startupType);
        IWebHostBuilder UseStartup<TStartup>(Func<WebHostBuilderContext, TStartup> startupFactory);
    }
}
