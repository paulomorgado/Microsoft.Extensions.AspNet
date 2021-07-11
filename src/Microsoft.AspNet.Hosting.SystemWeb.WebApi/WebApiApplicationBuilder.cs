using System;
using System.Collections.Generic;
using System.Web.Http;

namespace Microsoft.AspNet.Hosting.SystemWeb.WebApi
{
    internal sealed class WebApiApplicationBuilder : IWebApiApplicationBuilder
    {
        private List<Action<IServiceProvider, HttpConfiguration>> actions = new List<Action<IServiceProvider, HttpConfiguration>>();

        public WebApiApplicationBuilder(IServiceProvider services)
        {
            Services = services;
        }

        public IServiceProvider Services { get; }

        public IWebApiApplicationBuilder Configure(Action<IServiceProvider, HttpConfiguration> configureDelegate)
        {
            actions.Add(configureDelegate);
            return this;
        }

        public IWebApiApplicationBuilder Configure(Action<HttpConfiguration> configureDelegate)
        {
            actions.Add((_, c) => configureDelegate(c));
            return this;
        }

        internal void Configure(HttpConfiguration config)
        {
            foreach (var action in actions)
            {
                action(this.Services, config);
            }
        }
    }
}
