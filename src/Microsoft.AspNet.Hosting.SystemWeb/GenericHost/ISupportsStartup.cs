using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Microsoft.AspNet.Hosting.SystemWeb
{
    internal interface ISupportsStartup
    {
        IWebHostBuilder Configure(Action<WebHostBuilderContext, IApplicationBuilder> configure);
        IWebHostBuilder UseStartup(Type startupType);
        IWebHostBuilder UseStartup<TStartup>(Func<WebHostBuilderContext, TStartup> startupFactory);
    }
}
