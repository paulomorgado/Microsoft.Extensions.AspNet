using System;

namespace Microsoft.AspNet.Hosting.HttpRuntime.Mvc
{
    public class MvcApplicationBuilder : IMvcApplicationBuilder
    {
        public MvcApplicationBuilder(IServiceProvider services)
        {
            this.Services = services;
        }

        public IServiceProvider Services { get; }
    }
}
