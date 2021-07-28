using System;
using System.Collections.Generic;
using System.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNet.Hosting.SystemWeb
{
    internal sealed class PipelineManager : IPipelineManager, IDisposable
    {
        private readonly List<Pipeline> pipelines = new List<Pipeline>();
        private readonly ILogger logger;
        private readonly IServiceProvider services;

        public PipelineManager(
            IServiceProvider services,
            ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger("Microsoft.AspNet.Hosting.SystemWeb.Diagnostics");
            this.services = services;
        }

        public void Register(HttpApplication httpApplication)
        {
            this.pipelines.Add(new Pipeline(this.services, this.logger, httpApplication));
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            foreach (var pipeline in this.pipelines)
            {
                pipeline.Dispose();
            }
        }

        private sealed class Pipeline : IDisposable
        {
            private readonly ILogger logger;
            private readonly HttpApplication httpApplication;
            private readonly IDisposable httpApplicationLoggerScope;
            private readonly IServiceScope httpApplicationServiceScope;
            private IServiceScope httpRequestServiceScope;
            private Guid requestId;
            private IDisposable httpRequestLoggerScope;
            private Exception exception;
            private IHttpHandler handler;

            public Pipeline(IServiceProvider services, ILogger logger, HttpApplication httpApplication)
            {
                this.logger = logger;
                this.httpApplication = httpApplication;
                this.httpApplicationLoggerScope = this.logger.BeginApplicationInstanceScope(Guid.NewGuid());

                this.httpApplicationServiceScope = services.CreateScope();

                httpApplication.BeginRequest += OnBeginRequest;
                httpApplication.PostMapRequestHandler += OnPostMapRequestHandler; ;
                httpApplication.PreRequestHandlerExecute += OnPreRequestHandlerExecute; ;
                httpApplication.PostRequestHandlerExecute += OnPostRequestHandlerExecute; ;
                httpApplication.Error += OnError; ;
                httpApplication.EndRequest += OnEndRequest;
            }

            public void Dispose()
            {
                this.httpRequestLoggerScope?.Dispose();
                this.httpRequestServiceScope?.Dispose();
                this.httpApplicationLoggerScope.Dispose();
                this.httpApplicationServiceScope.Dispose();
            }

            private void OnBeginRequest(object sender, EventArgs e)
            {
                exception = null;

                var request = ((HttpApplication)sender).Context.Request;

                HostingEventSource.Log.RequestStart(request.HttpMethod, request.Path);

                httpRequestServiceScope = httpApplicationServiceScope.ServiceProvider.CreateScope();

                requestId = Guid.NewGuid();

                httpRequestLoggerScope = logger.BeginRequestLoggerScope(requestId);

                logger.LogBeginRequest(requestId, request.Url, request.HttpMethod);
            }

            private void OnPostMapRequestHandler(object sender, EventArgs e)
            {
                handler = httpApplication.Context.Handler;
                httpApplication.Context.Handler = null;
            }

            private void OnPreRequestHandlerExecute(object sender, EventArgs e)
            {
                httpApplication.Context.Handler = handler;
            }

            private void OnPostRequestHandlerExecute(object sender, EventArgs e)
            {
                if (exception != null)
                {
                    httpApplication.Context.AddError(exception);
                }
            }

            private void OnError(object sender, EventArgs e)
            {
                HostingEventSource.Log.UnhandledException();
                
                exception = httpApplication.Context.Error;

                httpApplication.Context.ClearError();
            }

            private void OnEndRequest(object sender, EventArgs e)
            {
                var response = ((HttpApplication)sender).Context.Response;

                logger.LogEndRequest(requestId, response.StatusCode);

                httpRequestLoggerScope.Dispose();
                httpRequestLoggerScope = null;

                httpRequestServiceScope?.Dispose();
                httpRequestServiceScope = null;

                this.requestId = default;

                HostingEventSource.Log.RequestStop();
            }
        }
    }
}
