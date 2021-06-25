using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.Hosting.HttpRuntime.Mvc
{
    public interface IMvcBuilder
    {
        IServiceCollection Services { get; }
    }
}
