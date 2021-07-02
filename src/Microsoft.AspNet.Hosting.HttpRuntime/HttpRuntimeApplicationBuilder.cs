using System;
using System.Collections.Generic;
using Microsoft.HttpRuntime.Hosting;

namespace Microsoft.AspNet.Hosting
{
    internal class HttpRuntimeApplicationBuilder : IHttpRuntimeApplicationBuilder
    {
        public HttpRuntimeApplicationBuilder(IServiceProvider serviceProvider)
        {
            Properties = new Dictionary<string, object>(StringComparer.Ordinal);
            ApplicationServices = serviceProvider;
        }

        public IServiceProvider ApplicationServices { get; set; }
        public IDictionary<string, object> Properties { get; }
    }
}
