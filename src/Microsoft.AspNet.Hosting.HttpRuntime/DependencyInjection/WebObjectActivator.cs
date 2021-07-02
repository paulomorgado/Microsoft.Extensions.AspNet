using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Web;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.Hosting.HttpRuntime.DependencyInjection
{
    internal sealed class WebObjectActivator : IServiceProvider, IServiceScopeFactory
    {
        private readonly ConcurrentDictionary<Type, ObjectFactory> unresolvedTypes;
        private readonly IServiceProvider serviceProvider;

        public WebObjectActivator(IServiceProvider serviceProvider)
            : this(serviceProvider, new ConcurrentDictionary<Type, ObjectFactory>())
        {
        }

        internal WebObjectActivator(IServiceProvider serviceProvider, ConcurrentDictionary<Type, ObjectFactory> unresolvedTypes)
        {
            this.serviceProvider = serviceProvider;
            this.unresolvedTypes = unresolvedTypes;
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(IServiceProvider))
            {
                return this;
            }

            if (serviceType == typeof(IServiceScopeFactory))
            {
                return this;
            }

            if (unresolvedTypes.TryGetValue(serviceType, out var objectFactory))
            {
                return objectFactory(serviceProvider, Array.Empty<object>());
            }

            if (ResolveServiceProvider().GetService(serviceType) is object service)
            {
                return service;
            }

            objectFactory = unresolvedTypes.GetOrAdd(serviceType, st =>
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

        public IServiceScope CreateScope() => new ServiceScrope(this);

        private IServiceProvider ResolveServiceProvider()
        {
            if (HttpContext.Current is HttpContext httpContext)
            {
                return httpContext.Items[HttpContextKeys.HttpContextServiceProviderKey] as IServiceProvider
                    ?? this.serviceProvider;
            }

            return this.serviceProvider;
        }

        private sealed class ServiceScrope : IServiceScope
        {
            private readonly IServiceScope scope;

            public ServiceScrope(WebObjectActivator webObjectActivator)
            {
                this.scope = webObjectActivator.serviceProvider.CreateScope();
                this.ServiceProvider = new WebObjectActivator(this.scope.ServiceProvider, webObjectActivator.unresolvedTypes);
            }

            public IServiceProvider ServiceProvider { get; }

            public void Dispose() => this.scope.Dispose();
        }
    }
}
