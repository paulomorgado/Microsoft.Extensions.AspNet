using System;
using Microsoft.AspNet.Hosting.HttpRuntime;
using Microsoft.AspNetCore.Builder;

namespace Microsoft.AspNet.Hosting
{
    internal sealed class GenericWebHostServiceOptions
    {
        public Action<IApplicationBuilder> ConfigureApplication { get; set; }

        public WebHostOptions WebHostOptions { get; set; } = default; // Always set when options resolved by DI
    }
}
