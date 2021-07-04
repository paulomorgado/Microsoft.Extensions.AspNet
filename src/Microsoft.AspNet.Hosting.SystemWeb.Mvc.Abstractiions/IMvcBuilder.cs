using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.Hosting.SystemWeb.Mvc
{
    public interface IMvcBuilder
    {
        IServiceCollection Services { get; }
    }
}
