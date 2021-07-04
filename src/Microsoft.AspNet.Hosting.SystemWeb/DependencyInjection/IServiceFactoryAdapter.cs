using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.Hosting.SystemWeb.DependencyInjection
{
    internal interface IServiceFactoryAdapter
    {
        object CreateBuilder(IServiceCollection services);

        IServiceProvider CreateServiceProvider(object containerBuilder);
    }
}
