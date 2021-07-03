using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Hosting.Internal;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.HttpRuntime.Hosting;

namespace Microsoft.AspNet.Hosting
{
    internal sealed class HttpRuntimeWebHostService : IHostedService
    {
        private readonly HttpRuntimeWebHostServiceOptions options;
        private readonly IHttpRuntimeApplicationBuilder applicationBuilderFactory;
        private readonly ILogger logger;

        public HttpRuntimeWebHostService(
            IOptions<HttpRuntimeWebHostServiceOptions> options,
            ILoggerFactory loggerFactory,
            IHttpRuntimeApplicationBuilder applicationBuilderFactory)
        {
            this.options = options.Value;
            this.applicationBuilderFactory = applicationBuilderFactory;
            this.logger = loggerFactory.CreateLogger("Microsoft.AspNet.Hosting.HttpRuntime.Diagnostics");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                var configure = options.ConfigureApplication;

                if (configure == null)
                {
                    throw new InvalidOperationException($"No application configured. Please specify an application via {nameof(IHttpRuntimeWebHostBuilder)}.{nameof(HttpRuntimeWebHostBuilderExtensions.UseStartup)}, {nameof(IHttpRuntimeWebHostBuilder)}.{nameof(HttpRuntimeWebHostBuilderExtensions.Configure)}, or specifying the startup assembly via {nameof(HttpRuntimeWebHostDefaults.StartupAssemblyKey)} in the web host configuration.");
                }

                configure(this.applicationBuilderFactory);
            }
            catch (Exception ex)
            {
                this.logger.ApplicationError(ex);

                if (!options.WebHostOptions.CaptureStartupErrors)
                {
                    throw;
                }
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
