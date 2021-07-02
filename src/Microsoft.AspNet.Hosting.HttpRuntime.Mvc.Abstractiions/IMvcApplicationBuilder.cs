using System;

namespace Microsoft.AspNet.Hosting.HttpRuntime.Mvc
{
    public interface IMvcApplicationBuilder
    {
        IServiceProvider Services { get; }
    }
}
