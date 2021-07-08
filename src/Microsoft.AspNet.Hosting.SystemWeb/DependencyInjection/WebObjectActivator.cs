using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.Hosting.SystemWeb.DependencyInjection
{
    internal sealed class WebObjectActivator : IWebObjectActivator, IDisposable
    {
        private static readonly Func<Type, ObjectFactory> createObjectFactory = new Func<Type, ObjectFactory>(CreateObjectFactory);
        private readonly ConcurrentDictionary<Type, ObjectFactory> unresolvedTypes;
        private readonly IServiceProvider parentServiceProvider;
        private readonly Lazy<ConcurrentQueue<IDisposable>> disposables = new Lazy<ConcurrentQueue<IDisposable>>(() => new ConcurrentQueue<IDisposable>(), LazyThreadSafetyMode.ExecutionAndPublication);
        private int isDisposed;

        public WebObjectActivator(IServiceProvider serviceProvider)
            : this(serviceProvider, new ConcurrentDictionary<Type, ObjectFactory>())
        {
        }

        private WebObjectActivator(IServiceProvider parentServiceProvider, ConcurrentDictionary<Type, ObjectFactory> unresolvedTypes)
        {
            this.parentServiceProvider = parentServiceProvider ?? throw new ArgumentNullException(nameof(parentServiceProvider));
            this.unresolvedTypes = unresolvedTypes;

            Debug.Assert(!(unresolvedTypes is null), $"{nameof(unresolvedTypes)} is null.");
        }

        public object GetService(Type serviceType)
        {
            if (Volatile.Read(ref this.isDisposed) == 1)
            {
                ThrowObjectDisposedException();
            }

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
                return CaptureDisposable(objectFactory(this, Array.Empty<object>()));
            }

            if (this.parentServiceProvider.GetService(serviceType) is object service)
            {
                return service;
            }

            objectFactory = unresolvedTypes.GetOrAdd(serviceType, createObjectFactory);

            return CaptureDisposable(objectFactory(this, Array.Empty<object>()));
        }

        private static ObjectFactory CreateObjectFactory(Type serviceType)
        {
            if (serviceType.IsAbstract || serviceType.IsInterface)
            {
                return new ObjectFactory((sp, args) => null);
            }

            var constructors = serviceType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            if (constructors is null || constructors.Length == 0)
            {
                return new ObjectFactory((sp, args) => Activator.CreateInstance(
                    serviceType,
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.CreateInstance,
                    null,
                    null,
                    null));
            }

            return ActivatorUtilities.CreateFactory(serviceType, Array.Empty<Type>());
        }

        public IServiceScope CreateScope() => new ServiceScrope(this);

        public void Dispose()
        {
            if (Interlocked.Exchange(ref this.isDisposed, 1) == 0
                && this.disposables.IsValueCreated)
            {
                foreach (var disposable in this.disposables.Value)
                {
                    disposable.Dispose();
                }
            }
        }

        private object CaptureDisposable(object service)
        {
            if (service is IDisposable disposable)
            {
                if (Volatile.Read(ref this.isDisposed) == 1)
                {
                    disposable.Dispose();

                    ThrowObjectDisposedException();
                }

                this.disposables.Value.Enqueue(disposable);
            }

            return service;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowObjectDisposedException()
        {
            throw new ObjectDisposedException(nameof(IServiceProvider));
        }

        private sealed class ServiceScrope : IServiceScope
        {
            private readonly IServiceScope scope;
            private readonly WebObjectActivator webObjectActivator;

            public ServiceScrope(WebObjectActivator parentWebObjectActivator)
            {
                scope = parentWebObjectActivator.parentServiceProvider.CreateScope();
                this.webObjectActivator = new WebObjectActivator(scope.ServiceProvider, parentWebObjectActivator.unresolvedTypes);
            }

            public IServiceProvider ServiceProvider => this.webObjectActivator;

            public void Dispose()
            {
                this.scope.Dispose();
                this.webObjectActivator.Dispose();
            }
        }
    }
}
