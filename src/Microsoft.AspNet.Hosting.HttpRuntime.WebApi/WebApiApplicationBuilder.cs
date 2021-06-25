using System;
using System.Collections.Generic;
using System.Web.Http;

namespace Microsoft.AspNet.Hosting.HttpRuntime.WebApi
{
    public class WebApiApplicationBuilder : IWebApiApplicationBuilder
    {
        private  List<Action<HttpConfiguration>> actions = new List<Action<HttpConfiguration>>();

        public WebApiApplicationBuilder(IServiceProvider services)
        {
            this.Services = services;
        }

        public IServiceProvider Services { get; }

        public IWebApiApplicationBuilder Configure(Action<HttpConfiguration> configureDelegate)
        {
            this.actions.Add(configureDelegate);
            return this;
        }

        internal void Configure(HttpConfiguration config)
        {
            foreach (var action in this.actions)
            {
                action(config);
            }
        }
    }
}
