using System;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Microsoft.AspNet.Hosting.SystemWeb
{
    internal sealed class SystemWebHostModule : IHttpModule
    {
        private static readonly Lazy<IHost> host = new Lazy<IHost>(BuildHost);
        private IServiceScope httpApplicationServiceScope;
        private IServiceScope httpContextServiceScope;

        public SystemWebHostModule()
        {
        }

        public void Init(HttpApplication httpApplication)
        {
            httpApplicationServiceScope = host.Value.Services.CreateScope();

            httpApplication.BeginRequest += (sender, e) =>
            {
                httpContextServiceScope = httpApplicationServiceScope.ServiceProvider.CreateScope();
            };

            httpApplication.EndRequest += (sender, e) =>
            {
                httpContextServiceScope?.Dispose();
                httpContextServiceScope = null;
            };
        }

        public void Dispose()
        {
            httpApplicationServiceScope.Dispose();
        }

        private static IHost BuildHost()
        {
            var builder = Host
                .CreateDefaultBuilder()
                .ConfigureSystemWebHostDefaults()
                .ConfigureHost()
                .ConfigureSystemWebWebHostDefaults(webBuilder => webBuilder.ConfigureWebHost());

            var host = builder.Build();

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
