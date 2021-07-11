using System;

namespace Microsoft.AspNet.Hosting.SystemWeb.Mvc
{
    internal sealed class MvcApplicationBuilder : IMvcApplicationBuilder
    {
        public MvcApplicationBuilder(IServiceProvider services)
        {
            Services = services;
        }

        public IServiceProvider Services { get; }
    }
}
