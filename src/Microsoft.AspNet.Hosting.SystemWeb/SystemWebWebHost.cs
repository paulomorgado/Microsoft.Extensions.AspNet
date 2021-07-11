using System.Web;
using Microsoft.AspNet.Hosting.SystemWeb;
using Microsoft.AspNet.Hosting.SystemWeb.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.AspNet.Hosting
{
    /// <summary>
    /// Provides convenience methods for creating instances of <see cref="IWebHost"/> and <see cref="IWebHostBuilder"/> with pre-configured defaults.
    /// </summary>
    public static class SystemWebWebHost
    {
        internal static void ConfigureSystemWebWebDefaults(IWebHostBuilder builder)
        {
#if DEBUG
            builder.ConfigureAppConfiguration((ctx, cb) =>
            {
            });
#endif

            builder.ConfigureServices((hostingContext, services) =>
            {
                services.TryAddSingleton<IWebObjectActivator, WebObjectActivator>();
            });
        }
    }
}
