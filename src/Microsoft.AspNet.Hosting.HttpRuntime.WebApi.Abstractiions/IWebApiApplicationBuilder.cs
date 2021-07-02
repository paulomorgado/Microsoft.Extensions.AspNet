using System;
using System.Web.Http;

namespace Microsoft.AspNet.Hosting.HttpRuntime.WebApi
{
    public interface IWebApiApplicationBuilder
    {
        IServiceProvider Services { get; }

        IWebApiApplicationBuilder Configure(Action<HttpConfiguration> configureDelegate);
    }
}
