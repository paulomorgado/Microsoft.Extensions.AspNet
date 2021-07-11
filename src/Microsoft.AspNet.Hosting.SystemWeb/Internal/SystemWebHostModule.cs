using System;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Hosting.SystemWeb.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNet.Hosting.SystemWeb
{
    internal sealed class SystemWebHostModule : IHttpModule
    {
        private static readonly Lazy<IHost> host = new Lazy<IHost>(BuildHost);
        private static ILogger logger;
        private IServiceScope httpApplicationServiceScope;
        private IDisposable httpApplicationLoggerScope;
        private IServiceScope httpRequestServiceScope;
        private IDisposable httpRequestLoggerScope;
        private Guid requestId;

        public SystemWebHostModule()
        {
        }

        public void Init(HttpApplication httpApplication)
        {
            this.httpApplicationServiceScope = host.Value.Services.CreateScope();

            this.httpApplicationLoggerScope = logger.BeginApplicationInstanceScope(Guid.NewGuid());

            httpApplication.BeginRequest += (sender, e) =>
            {
                var request = ((HttpApplication)sender).Context.Request;

                httpRequestServiceScope = httpApplicationServiceScope.ServiceProvider.CreateScope();

                requestId = Guid.NewGuid();

                httpRequestLoggerScope = logger.BeginRequestLoggerScope(requestId);

                logger.LogBeginRequest(requestId, request.Url, request.HttpMethod);
            };

            httpApplication.EndRequest += (sender, e) =>
            {
                var response = ((HttpApplication)sender).Context.Response;

                logger.LogEndRequest(requestId, response.StatusCode);

                httpRequestLoggerScope.Dispose();
                httpRequestLoggerScope = null;

                httpRequestServiceScope?.Dispose();
                httpRequestServiceScope = null;

                this.requestId = Guid.Empty;
            };
        }

        public void Dispose()
        {
            httpRequestLoggerScope?.Dispose();
            httpRequestServiceScope?.Dispose();
            httpApplicationLoggerScope.Dispose();
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

            logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Microsoft.AspNet.Hosting.SystemWeb.Diagnostics");

            HttpRuntime.WebObjectActivator = host.Services.GetRequiredService<IWebObjectActivator>();

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
