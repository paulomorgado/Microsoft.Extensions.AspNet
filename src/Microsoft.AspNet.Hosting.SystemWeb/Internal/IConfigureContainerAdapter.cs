using Microsoft.Extensions.Hosting;

namespace Microsoft.AspNet.Hosting.SystemWeb
{
    internal interface IConfigureContainerAdapter
    {
        void ConfigureContainer(HostBuilderContext hostContext, object containerBuilder);
    }
}
