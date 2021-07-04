using Microsoft.AspNetCore.Hosting;

namespace Microsoft.AspNet.Hosting
{
    /// <summary>
    /// Provides convenience methods for creating instances of <see cref="IHttpRuntimeWebHost"/> and <see cref="IWebHostBuilder"/> with pre-configured defaults.
    /// </summary>
    public static class SystemWebWebHost
    {
        internal static void ConfigureSystemWebWebDefaults(IWebHostBuilder builder)
        {
#if DEBUG
            builder.ConfigureAppConfiguration((ctx, cb) =>
            {
            });

            builder.ConfigureServices((hostingContext, services) =>
            {
            });
        }
#endif
    }
}
