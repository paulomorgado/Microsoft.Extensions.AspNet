using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.Hosting.SystemWeb.WebApi
{
    internal sealed class WebApiBuilder : IWebApiBuilder
    {
        public WebApiBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }
    }
}
