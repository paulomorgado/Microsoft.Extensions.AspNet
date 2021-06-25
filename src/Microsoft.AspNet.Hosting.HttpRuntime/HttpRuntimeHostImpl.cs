using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Hosting.HttpRuntime;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNet.Hosting
{
    internal sealed class HttpRuntimeHostImpl : IHttpRuntimeHost, IHost
    {
        private HostingEnvironment hostingEnvironment;
        private PhysicalFileProvider defaultProvider;
        private IHostApplicationLifetime hostApplicationLifetime;
        private ILogger<HttpRuntimeHostImpl> logger;
        private IHostLifetime hostLifetime;
        private IOptions<HostOptions> options;

        public HttpRuntimeHostImpl(
            IServiceProvider appServices,
            HostingEnvironment hostingEnvironment,
            PhysicalFileProvider defaultProvider,
            IHostApplicationLifetime hostApplicationLifetime,
            ILogger<HttpRuntimeHostImpl> logger,
            IHostLifetime hostLifetime,
            IOptions<HostOptions> options)
        {
            this.Services = appServices;
            this.hostingEnvironment = hostingEnvironment;
            this.defaultProvider = defaultProvider;
            this.hostApplicationLifetime = hostApplicationLifetime;
            this.logger = logger;
            this.hostLifetime = hostLifetime;
            this.options = options;
        }

        public IServiceProvider Services { get; }

        public void Start(HttpApplication httpApplication)
        {
            if (httpApplication is null)
            {
                throw new ArgumentNullException(nameof(httpApplication));
            }

            new HttpRuntimeHostModule(this.Services).Init(httpApplication);
        }

        public void Stop()
        {
        }

        Task IHost.StartAsync(CancellationToken cancellationToken)
        {
            throw new InvalidOperationException();
        }

        Task IHost.StopAsync(CancellationToken cancellationToken)
        {
            throw new InvalidOperationException();
        }

        public void Dispose()
        {
            defaultProvider.Dispose();
        }
    }
}
