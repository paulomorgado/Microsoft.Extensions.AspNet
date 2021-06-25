using System;
using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.Hosting.HttpRuntime.DependencyInjection
{
    internal sealed class HttpRuntimeRootServiceProvider
    {
        private readonly ConcurrentDictionary<Type, ObjectFactory> unresolvedTypes = new ConcurrentDictionary<Type, ObjectFactory>();
        private readonly IServiceProvider serviceProvider;
        private readonly ServiceScopeFactory serviceScopeFactory;

        public HttpRuntimeRootServiceProvider(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            serviceScopeFactory = new ServiceScopeFactory(this, this.serviceProvider.GetRequiredService<IServiceScopeFactory>());
        }

        public object GetRootService(Type serviceType)
        {
            if (serviceType == typeof(IServiceScopeFactory))
            {
                return serviceScopeFactory;
            }

            if (unresolvedTypes.TryGetValue(serviceType, out var objectFactory))
            {
                return objectFactory(serviceProvider, Array.Empty<object>());
            }

            return null;
        }

        public object GetOrAddRootService(Type serviceType)
        {
            var objectFactory = unresolvedTypes.GetOrAdd(serviceType, st =>
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

            return objectFactory(serviceProvider, Array.Empty<object>());
        }

        private sealed class ServiceScopeFactory : IServiceScopeFactory
        {
            private readonly IServiceScopeFactory serviceScopeFactory;
            private readonly HttpRuntimeRootServiceProvider aspNetRootServiceProvider;

            public ServiceScopeFactory(HttpRuntimeRootServiceProvider aspNetRootServiceProvider, IServiceScopeFactory serviceScopeFactory)
            {
                this.aspNetRootServiceProvider = aspNetRootServiceProvider;
                this.serviceScopeFactory = serviceScopeFactory;
            }

            public IServiceScope CreateScope() => new ServiceScope(aspNetRootServiceProvider, serviceScopeFactory.CreateScope());

            private sealed class ServiceScope : IServiceScope
            {
                private readonly IServiceScope serviceScope;
                private readonly HttpRuntimeRootServiceProvider rootServiceProvider;

                public ServiceScope(HttpRuntimeRootServiceProvider aspNetRootServiceProvider, IServiceScope serviceScope)
                {
                    rootServiceProvider = aspNetRootServiceProvider;
                    this.serviceScope = serviceScope;

                    ServiceProvider = new HttpRuntimeServiceProvider(rootServiceProvider, this.serviceScope.ServiceProvider);
                }

                public IServiceProvider ServiceProvider { get; }

                public void Dispose() => serviceScope.Dispose();
            }
        }
    }
}
