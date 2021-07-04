using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Microsoft.AspNet.Hosting
{
    public static class SystemWebHosting
    {
        private static Action<IHostBuilder> configureHostAction;
        private static Action<IWebHostBuilder> configureWebHostAction;

        public static void ConfigureHost(Action<IHostBuilder> configureAction) => configureHostAction = configureAction;

        public static void ConfigureWebHost(Action<IWebHostBuilder> configureAction) => configureWebHostAction = configureAction;

        internal static IHostBuilder ConfigureHost(this IHostBuilder builder)
        {
            configureHostAction?.Invoke(builder);
            return builder;
        }

        internal static IWebHostBuilder ConfigureWebHost(this IWebHostBuilder builder)
        {
            configureWebHostAction?.Invoke(builder);
            return builder;
        }
    }
}
