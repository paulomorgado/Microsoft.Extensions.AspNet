using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Hosting.SystemWeb.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNet.Hosting.SystemWeb
{
    internal sealed class GenericWebHostService : IHostedService
    {
        private readonly GenericWebHostServiceOptions options;
        private readonly IApplicationBuilderFactory applicationBuilderFactory;
        private readonly ILogger logger;

        public GenericWebHostService(
            IOptions<GenericWebHostServiceOptions> options,
            ILoggerFactory loggerFactory,
            IApplicationBuilderFactory applicationBuilderFactory,
            IEnumerable<IStartupFilter> startupFilters)
        {
            this.options = options.Value;
            this.applicationBuilderFactory = applicationBuilderFactory;
            StartupFilters = startupFilters;
            logger = loggerFactory.CreateLogger("Microsoft.AspNet.Hosting.SystemWeb.Diagnostics");
        }

        public IEnumerable<IStartupFilter> StartupFilters { get; }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                var configure = options.ConfigureApplication;

                if (configure == null)
                {
                    throw new InvalidOperationException($"No application configured. Please specify an application via {nameof(IWebHostBuilder)}.{nameof(SystemWebWebHostBuilderExtensions.UseStartup)}, {nameof(IWebHostBuilder)}.{nameof(SystemWebWebHostBuilderExtensions.Configure)}, or specifying the startup assembly via {nameof(WebHostDefaults.StartupAssemblyKey)} in the web host configuration.");
                }

                var builder = applicationBuilderFactory.CreateBuilder(new FeatureCollection());

                foreach (var filter in StartupFilters.Reverse())
                {
                    configure = filter.Configure(configure);
                }

                configure(builder);
            }
            catch (Exception ex)
            {
                logger.ApplicationError(ex);

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
