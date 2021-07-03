namespace Microsoft.AspNet.Hosting
{
    /// <summary>
    /// Provides convenience methods for creating instances of <see cref="IHttpRuntimeWebHost"/> and <see cref="IHttpRuntimeWebHostBuilder"/> with pre-configured defaults.
    /// </summary>
    public static class HttpRuntimeWebHost
    {
        internal static void ConfigureHttpRuntimeWebDefaults(IHttpRuntimeWebHostBuilder builder)
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
