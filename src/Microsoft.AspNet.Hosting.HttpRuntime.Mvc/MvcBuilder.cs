using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.Hosting.HttpRuntime.Mvc
{
    public class MvcBuilder : IMvcBuilder
    {
        public MvcBuilder(IServiceCollection services)
        {
            this.Services = services;
        }

        public IServiceCollection Services { get; }
    }
}
