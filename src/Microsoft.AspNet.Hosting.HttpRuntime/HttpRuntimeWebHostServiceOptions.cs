using System;
using Microsoft.AspNet.Hosting.HttpRuntime;
using Microsoft.HttpRuntime.Hosting;

namespace Microsoft.AspNet.Hosting
{
    internal class HttpRuntimeWebHostServiceOptions
    {
        public Action<IHttpRuntimeApplicationBuilder> ConfigureApplication { get; set; }

        public WebHostOptions WebHostOptions { get; set; } = default; // Always set when options resolved by DI
    }
}
