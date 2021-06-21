using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection.AspNet
{
    internal sealed class AspNetRootServiceProvider
    {
        private readonly ConcurrentDictionary<Type, ObjectFactory> unresolvedTypes = new ConcurrentDictionary<Type, ObjectFactory>();
        private readonly IServiceProvider serviceProvider;
        private readonly ServiceScopeFactory serviceScopeFactory;

        public AspNetRootServiceProvider(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.serviceScopeFactory = new ServiceScopeFactory(this, this.serviceProvider.GetRequiredService<IServiceScopeFactory>());
        }

        public object GetRootService(Type serviceType)
        {
            if (serviceType == typeof(IServiceScopeFactory))
            {
                return this.serviceScopeFactory;
            }

            if (this.unresolvedTypes.TryGetValue(serviceType, out var objectFactory))
            {
                return objectFactory(this.serviceProvider, Array.Empty<object>());
            }

            return null;
        }

        public object GetOrAddRootService(Type serviceType)
        {
            var objectFactory = this.unresolvedTypes.GetOrAdd(serviceType, st =>
            {
                if (serviceType.IsAbstract || serviceType.IsInterface)
                {
                    return new ObjectFactory((sp, args) => null);
                }

                if (serviceType.IsPublic)
                {
                    try
                    {
                        return ActivatorUtilities.CreateFactory(serviceType, Array.Empty<Type>());
                    }
                    catch
                    {
                    }
                }

                return new ObjectFactory((sp, args) => Activator.CreateInstance(
                    serviceType,
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.CreateInstance,
                    null,
                    null,
                    null));
            });

            return objectFactory(this.serviceProvider, Array.Empty<object>());
        }

        private sealed class ServiceScopeFactory : IServiceScopeFactory
        {
            private readonly IServiceScopeFactory serviceScopeFactory;
            private readonly AspNetRootServiceProvider aspNetRootServiceProvider;

            public ServiceScopeFactory(AspNetRootServiceProvider aspNetRootServiceProvider, IServiceScopeFactory serviceScopeFactory)
            {
                this.aspNetRootServiceProvider = aspNetRootServiceProvider;
                this.serviceScopeFactory = serviceScopeFactory;
            }

            public IServiceScope CreateScope() => new ServiceScope(this.aspNetRootServiceProvider, this.serviceScopeFactory.CreateScope());

            private sealed class ServiceScope : IServiceScope
            {
                private readonly IServiceScope serviceScope;
                private readonly AspNetRootServiceProvider aspNetRootServiceProvider;

                public ServiceScope(AspNetRootServiceProvider aspNetRootServiceProvider, IServiceScope serviceScope)
                {
                    this.aspNetRootServiceProvider = aspNetRootServiceProvider;
                    this.serviceScope = serviceScope;

                    this.ServiceProvider = new AspNetServiceProvider(this.aspNetRootServiceProvider, this.serviceScope.ServiceProvider);
                }

                public IServiceProvider ServiceProvider { get; }

                public void Dispose() => this.serviceScope.Dispose();
            }
        }
    }
}
