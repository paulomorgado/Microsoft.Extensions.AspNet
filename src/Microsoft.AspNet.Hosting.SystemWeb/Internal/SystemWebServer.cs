using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Hosting.SystemWeb.DependencyInjection;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Features;

namespace Microsoft.AspNet.Hosting.SystemWeb
{
    internal sealed class SystemWebServer : IServer
    {
        private readonly IWebObjectActivator webObjectActivator;

        public SystemWebServer(IWebObjectActivator webObjectActivator)
        {
            this.webObjectActivator = webObjectActivator ?? throw new ArgumentNullException(nameof(webObjectActivator));
        }

        /// <inheritdoc/>
        public IFeatureCollection Features { get; } = new FeatureCollection();

        /// <inheritdoc/>
        public Task StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken)
        {
            HttpRuntime.WebObjectActivator = this.webObjectActivator;

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }
    }
}
