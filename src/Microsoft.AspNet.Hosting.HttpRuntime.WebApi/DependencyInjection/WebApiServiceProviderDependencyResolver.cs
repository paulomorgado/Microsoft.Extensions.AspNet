using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;

namespace Microsoft.Extensions.DependencyInjection.AspNetWebApi
{
    internal sealed class WebApiServiceProviderDependencyResolver : IDependencyResolver
    {
        private readonly IServiceProvider serviceProvider;

        public WebApiServiceProviderDependencyResolver(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IDependencyScope BeginScope() => new ServiceProviderWebApiDependencyScope(this.serviceProvider.CreateScope());

        public object GetService(System.Type serviceType) => this.serviceProvider.GetService(serviceType);

        public IEnumerable<object> GetServices(System.Type serviceType) => this.serviceProvider.GetServices(serviceType);

        public void Dispose()
        {
        }

        private sealed class ServiceProviderWebApiDependencyScope : IDependencyScope
        {
            private readonly IServiceScope serviceScope;

            public ServiceProviderWebApiDependencyScope(IServiceScope serviceScope)
            {
                this.serviceScope = serviceScope;
            }

            public object GetService(Type serviceType) => this.serviceScope.ServiceProvider.GetService(serviceType);

            public IEnumerable<object> GetServices(Type serviceType) => this.serviceScope.ServiceProvider.GetServices(serviceType);

            public void Dispose()
            {
                this.serviceScope.Dispose();
            }
        }
    }
}
