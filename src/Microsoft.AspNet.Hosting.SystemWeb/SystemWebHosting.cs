using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Microsoft.AspNet.Hosting.SystemWeb
{
    public static class SystemWebHosting
    {
        private static readonly Lazy<IHost> host = new Lazy<IHost>(BuildHost);

        private static Action<IHostBuilder> configureHostAction;
        private static Action<IWebHostBuilder> configureWebHostAction;

        public static void ConfigureHost(Action<IHostBuilder> configureAction)
        {
            if (host.IsValueCreated)
            {
                throw new InvalidOperationException("The host has already been built.");
            }

            configureHostAction = configureAction;
        }

        public static void ConfigureWebHost(Action<IWebHostBuilder> configureAction)
        {
            if (host.IsValueCreated)
            {
                throw new InvalidOperationException("The host has already been built.");
            }

            configureWebHostAction = configureAction;
        }

        public static IHost GetHost() => host.Value;

        private static IHostBuilder ConfigureHost(this IHostBuilder builder)
        {
            configureHostAction?.Invoke(builder);
            return builder;
        }

        private static IWebHostBuilder ConfigureWebHost(this IWebHostBuilder builder)
        {
            configureWebHostAction?.Invoke(builder);
            return builder;
        }

        private static IHost BuildHost()
        {
            var builder = Host
                .CreateDefaultBuilder()
                .ConfigureSystemWebHostDefaults()
                .ConfigureHost()
                .ConfigureSystemWebWebHostDefaults(webBuilder => webBuilder.ConfigureWebHost());

            var host = builder.Build();

            //HttpRuntime.WebObjectActivator = host.Services.GetRequiredService<IWebObjectActivator>();

            // try stopping the host on application pool shutdown
            System.Web.Hosting.HostingEnvironment.QueueBackgroundWorkItem(async ct =>
            {
                try
                {
                    var tcs = new TaskCompletionSource<bool>();
                    using (ct.Register(() => tcs.SetResult(true)))
                    {
                        await tcs.Task;
                    }

                    await host.StopAsync();
                }
                catch { }
            });

            host.Start();

            return host;
        }
    }
}
