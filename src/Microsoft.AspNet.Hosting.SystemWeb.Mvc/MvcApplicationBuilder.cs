using System;

namespace Microsoft.AspNet.Hosting.SystemWeb.Mvc
{
    public class MvcApplicationBuilder : IMvcApplicationBuilder
    {
        public MvcApplicationBuilder(IServiceProvider services)
        {
            Services = services;
        }

        public IServiceProvider Services { get; }
    }
}
