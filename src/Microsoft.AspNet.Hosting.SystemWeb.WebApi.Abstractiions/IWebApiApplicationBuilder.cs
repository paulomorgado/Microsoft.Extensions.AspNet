using System;
using System.Web.Http;

namespace Microsoft.AspNet.Hosting.SystemWeb.WebApi
{
    public interface IWebApiApplicationBuilder
    {
        IServiceProvider Services { get; }

        IWebApiApplicationBuilder Configure(Action<HttpConfiguration> configureDelegate);
    }
}
