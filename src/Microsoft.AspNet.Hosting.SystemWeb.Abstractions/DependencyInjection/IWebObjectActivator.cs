using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.Hosting.SystemWeb.DependencyInjection
{
    public interface IWebObjectActivator: IServiceProvider, IServiceScopeFactory
    {
    }
}