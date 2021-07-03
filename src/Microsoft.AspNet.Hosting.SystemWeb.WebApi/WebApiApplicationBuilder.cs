using System;
using System.Collections.Generic;
using System.Web.Http;

namespace Microsoft.AspNet.Hosting.SystemWeb.WebApi
{
    public class WebApiApplicationBuilder : IWebApiApplicationBuilder
    {
        private List<Action<HttpConfiguration>> actions = new List<Action<HttpConfiguration>>();

        public WebApiApplicationBuilder(IServiceProvider services)
        {
            Services = services;
        }

        public IServiceProvider Services { get; }

        public IWebApiApplicationBuilder Configure(Action<HttpConfiguration> configureDelegate)
        {
            actions.Add(configureDelegate);
            return this;
        }

        internal void Configure(HttpConfiguration config)
        {
            foreach (var action in actions)
            {
                action(config);
            }
        }
    }
}
