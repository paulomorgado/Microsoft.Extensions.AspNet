using System;

namespace Microsoft.AspNet.Hosting.SystemWeb.Mvc
{
    public interface IMvcApplicationBuilder
    {
        IServiceProvider Services { get; }
    }
}
