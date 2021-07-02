using Microsoft.Extensions.Hosting;

namespace Microsoft.AspNet.Hosting.HttpRuntime
{
    internal interface IConfigureContainerAdapter
    {
        void ConfigureContainer(HostBuilderContext hostContext, object containerBuilder);
    }
}
