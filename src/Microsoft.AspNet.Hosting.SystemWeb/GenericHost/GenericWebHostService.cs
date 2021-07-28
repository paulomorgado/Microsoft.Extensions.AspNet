using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Hosting.SystemWeb.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNet.Hosting.SystemWeb
{
    internal sealed class GenericWebHostService : IHostedService
    {
        private readonly GenericWebHostServiceOptions options;
        private readonly IServer server;
        private readonly IApplicationBuilderFactory applicationBuilderFactory;
        private readonly ILogger logger;
        private readonly IEnumerable<IStartupFilter> startupFilters;
        private readonly IWebHostEnvironment hostingEnvironment;

        public GenericWebHostService(
            IOptions<GenericWebHostServiceOptions> options,
            IServer server,
            ILoggerFactory loggerFactory,
            IApplicationBuilderFactory applicationBuilderFactory,
            IEnumerable<IStartupFilter> startupFilters,
            IWebHostEnvironment hostingEnvironment)
        {
            this.options = options.Value;
            this.server = server;
            this.applicationBuilderFactory = applicationBuilderFactory;
            this.startupFilters = startupFilters;
            this.hostingEnvironment = hostingEnvironment;
            this.logger = loggerFactory.CreateLogger("Microsoft.AspNet.Hosting.SystemWeb.Diagnostics");
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            HostingEventSource.Log.HostStart();

            RequestDelegate application = null;

            try
            {
                var configure = options.ConfigureApplication;

                if (configure == null)
                {
                    throw new InvalidOperationException($"No application configured. Please specify an application via {nameof(IWebHostBuilder)}.{nameof(SystemWebWebHostBuilderExtensions.UseStartup)}, {nameof(IWebHostBuilder)}.{nameof(SystemWebWebHostBuilderExtensions.Configure)}, or specifying the startup assembly via {nameof(WebHostDefaults.StartupAssemblyKey)} in the web host configuration.");
                }

                var builder = applicationBuilderFactory.CreateBuilder(new FeatureCollection());

                foreach (var filter in startupFilters.Reverse())
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

                application = BuildErrorPageApplication(ex);
            }

            var httpApplication = new SystemWebHttpApplication();

            await this.server.StartAsync(httpApplication, cancellationToken);

            if (this.logger.IsEnabled(LogLevel.Debug))
            {
                foreach (var assembly in this.options.WebHostOptions.GetFinalHostingStartupAssemblies())
                {
                    this.logger.LogDebug("Loaded hosting startup assembly {assemblyName}", assembly);
                }
            }

            if (this.options.HostingStartupExceptions != null)
            {
                foreach (var exception in this.options.HostingStartupExceptions.InnerExceptions)
                {
                    this.logger.HostingStartupAssemblyError(exception);
                }
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                await this.server.StopAsync(cancellationToken);
            }
            finally
            {
                HostingEventSource.Log.HostStop();
            }
        }

        private RequestDelegate BuildErrorPageApplication(Exception exception)
        {
            if (exception is TargetInvocationException tae)
            {
                exception = tae.InnerException;
            }

            var showDetailedErrors = hostingEnvironment.IsDevelopment() || this.options.WebHostOptions.DetailedErrors;

            /*
            var model = new ErrorPageModel
            {
                RuntimeDisplayName = RuntimeInformation.FrameworkDescription
            };
            var systemRuntimeAssembly = typeof(System.ComponentModel.DefaultValueAttribute).GetTypeInfo().Assembly;
            var assemblyVersion = new AssemblyName(systemRuntimeAssembly.FullName).Version.ToString();
            var clrVersion = assemblyVersion;
            model.RuntimeArchitecture = RuntimeInformation.ProcessArchitecture.ToString();
            var currentAssembly = typeof(ErrorPage).GetTypeInfo().Assembly;
            model.CurrentAssemblyVesion = currentAssembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                .InformationalVersion;
            model.ClrVersion = clrVersion;
            model.OperatingSystemDescription = RuntimeInformation.OSDescription;
            model.ShowRuntimeDetails = showDetailedErrors;

            if (showDetailedErrors)
            {
                var exceptionDetailProvider = new ExceptionDetailsProvider(
                    HostingEnvironment.ContentRootFileProvider,
                    this.logger,
                    sourceCodeLineCount: 6);

                model.ErrorDetails = exceptionDetailProvider.GetDetails(exception);
            }
            else
            {
                model.ErrorDetails = Array.Empty<ExceptionDetails>();
            }

            var errorPage = new ErrorPage(model);
            return context =>
            {
                context.Response.StatusCode = 500;
                context.Response.Headers[HeaderNames.CacheControl] = "no-cache,no-store";
                context.Response.Headers[HeaderNames.Pragma] = "no-cache";
                context.Response.ContentType = "text/html; charset=utf-8";
                return errorPage.ExecuteAsync(context);
            };
            */

            return null;
        }
    }
}
