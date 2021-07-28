using System;
using Microsoft.AspNetCore.Builder;

namespace Microsoft.AspNet.Hosting.SystemWeb
{
    internal sealed class GenericWebHostServiceOptions
    {
        public Action<IApplicationBuilder> ConfigureApplication { get; set; }

        public WebHostOptions WebHostOptions { get; set; }

        public AggregateException HostingStartupExceptions { get; set; }
    }
}
