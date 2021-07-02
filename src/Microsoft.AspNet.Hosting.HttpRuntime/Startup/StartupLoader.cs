using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.Hosting.HttpRuntime.Startup
{
    internal class StartupLoader
    {
        public static StartupMethods LoadMethods(IServiceProvider hostingServiceProvider, Type startupType, string environmentName, object instance = null)
        {
            var configureMethod = FindConfigureDelegate(startupType, environmentName);

            var servicesMethod = FindConfigureServicesDelegate(startupType, environmentName);
            var configureContainerMethod = FindConfigureContainerDelegate(startupType, environmentName);

            if (instance == null && (!configureMethod.MethodInfo.IsStatic || servicesMethod?.MethodInfo != null && !servicesMethod.MethodInfo.IsStatic))
            {
                instance = ActivatorUtilities.GetServiceOrCreateInstance(hostingServiceProvider, startupType);
            }

            // The type of the TContainerBuilder. If there is no ConfigureContainer method we can just use object as it's not
            // going to be used for anything.
            var type = configureContainerMethod.MethodInfo != null ? configureContainerMethod.GetContainerType() : typeof(object);

            var builder = (ConfigureServicesDelegateBuilder)Activator.CreateInstance(
                typeof(ConfigureServicesDelegateBuilder<>).MakeGenericType(type),
                hostingServiceProvider,
                servicesMethod,
                configureContainerMethod,
                instance);

            return new StartupMethods(instance, configureMethod.Build(instance), builder.Build());
        }

        public static Type FindStartupType(string startupAssemblyName, string environmentName)
        {
            if (string.IsNullOrEmpty(startupAssemblyName))
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "A startup method, startup type or startup assembly is required. If specifying an assembly, '{0}' cannot be null or empty.",
                        nameof(startupAssemblyName)),
                        nameof(startupAssemblyName));
            }

            var assembly = Assembly.Load(new AssemblyName(startupAssemblyName));
            if (assembly == null)
            {
                throw new InvalidOperationException($"The assembly '{startupAssemblyName}' failed to load.");
            }

            var startupNameWithEnv = "Startup" + environmentName;
            var startupNameWithoutEnv = "Startup";

            // Check the most likely places first
            var type =
                assembly.GetType(startupNameWithEnv) ??
                assembly.GetType(startupAssemblyName + "." + startupNameWithEnv) ??
                assembly.GetType(startupNameWithoutEnv) ??
                assembly.GetType(startupAssemblyName + "." + startupNameWithoutEnv);

            if (type == null)
            {
                // Full scan
                var definedTypes = assembly.DefinedTypes.ToList();

                var startupType1 = definedTypes.Where(info => info.Name.Equals(startupNameWithEnv, StringComparison.OrdinalIgnoreCase));
                var startupType2 = definedTypes.Where(info => info.Name.Equals(startupNameWithoutEnv, StringComparison.OrdinalIgnoreCase));

                var typeInfo = startupType1.Concat(startupType2).FirstOrDefault();
                if (typeInfo != null)
                {
                    type = typeInfo.AsType();
                }
            }

            if (type == null)
            {
                throw new InvalidOperationException(string.Format(
                    CultureInfo.CurrentCulture,
                    "A type named '{0}' or '{1}' could not be found in assembly '{2}'.",
                    startupNameWithEnv,
                    startupNameWithoutEnv,
                    startupAssemblyName));
            }

            return type;
        }

        public static bool HasConfigureServicesIServiceProviderDelegate(Type startupType, string environmentName)
        {
            return null != FindMethod(startupType, "Configure{0}Services", environmentName, typeof(IServiceProvider), required: false);
        }

        private static MethodInfo FindMethod(Type startupType, string methodName, string environmentName, Type returnType = null, bool required = true)
        {
            var methodNameWithEnv = string.Format(CultureInfo.InvariantCulture, methodName, environmentName);
            var methodNameWithNoEnv = string.Format(CultureInfo.InvariantCulture, methodName, "");

            var methods = startupType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            var selectedMethods = methods.Where(method => method.Name.Equals(methodNameWithEnv, StringComparison.OrdinalIgnoreCase)).ToList();
            if (selectedMethods.Count > 1)
            {
                throw new InvalidOperationException($"Having multiple overloads of method '{methodNameWithEnv}' is not supported.");
            }
            if (selectedMethods.Count == 0)
            {
                selectedMethods = methods.Where(method => method.Name.Equals(methodNameWithNoEnv, StringComparison.OrdinalIgnoreCase)).ToList();
                if (selectedMethods.Count > 1)
                {
                    throw new InvalidOperationException($"Having multiple overloads of method '{methodNameWithNoEnv}' is not supported.");
                }
            }

            var methodInfo = selectedMethods.FirstOrDefault();
            if (methodInfo == null)
            {
                if (required)
                {
                    throw new InvalidOperationException(string.Format(
                        CultureInfo.CurrentCulture,
                        "A public method named '{0}' or '{1}' could not be found in the '{2}' type.",
                        methodNameWithEnv,
                        methodNameWithNoEnv,
                        startupType.FullName));

                }
                return null;
            }
            if (returnType != null && methodInfo.ReturnType != returnType)
            {
                if (required)
                {
                    throw new InvalidOperationException(string.Format(
                        CultureInfo.CurrentCulture,
                        "The '{0}' method in the type '{1}' must have a return type of '{2}'.",
                        methodInfo.Name,
                        startupType.FullName,
                        returnType.Name));
                }
                return null;
            }
            return methodInfo;
        }

        public static ConfigureBuilder FindConfigureDelegate(Type startupType, string environmentName)
        {
            var configureMethod = FindMethod(startupType, "Configure{0}", environmentName, typeof(void), required: true);
            return new ConfigureBuilder(configureMethod);
        }

        public static ConfigureContainerBuilder FindConfigureContainerDelegate(Type startupType, string environmentName)
        {
            var configureMethod = FindMethod(startupType, "Configure{0}Container", environmentName, typeof(void), required: false);
            return new ConfigureContainerBuilder(configureMethod);
        }

        public static ConfigureServicesBuilder FindConfigureServicesDelegate(Type startupType, string environmentName)
        {
            var servicesMethod = FindMethod(startupType, "Configure{0}Services", environmentName, typeof(IServiceProvider), required: false)
                ?? FindMethod(startupType, "Configure{0}Services", environmentName, typeof(void), required: false);
            return new ConfigureServicesBuilder(servicesMethod);
        }

        private abstract class ConfigureServicesDelegateBuilder
        {
            public abstract Func<IServiceCollection, IServiceProvider> Build();
        }

        private class ConfigureServicesDelegateBuilder<TContainerBuilder> : ConfigureServicesDelegateBuilder where TContainerBuilder : class
        {
            public ConfigureServicesDelegateBuilder(
                IServiceProvider hostingServiceProvider,
                ConfigureServicesBuilder configureServicesBuilder,
                ConfigureContainerBuilder configureContainerBuilder,
                object instance)
            {
                HostingServiceProvider = hostingServiceProvider;
                ConfigureServicesBuilder = configureServicesBuilder;
                ConfigureContainerBuilder = configureContainerBuilder;
                Instance = instance;
            }

            public IServiceProvider HostingServiceProvider { get; }
            public ConfigureServicesBuilder ConfigureServicesBuilder { get; }
            public ConfigureContainerBuilder ConfigureContainerBuilder { get; }
            public object Instance { get; }

            public override Func<IServiceCollection, IServiceProvider> Build()
            {
                ConfigureServicesBuilder.StartupServiceFilters = BuildStartupServicesFilterPipeline;
                var configureServicesCallback = ConfigureServicesBuilder.Build(Instance);

                ConfigureContainerBuilder.ConfigureContainerFilters = ConfigureContainerPipeline;
                var configureContainerCallback = ConfigureContainerBuilder.Build(Instance);

                return ConfigureServices(configureServicesCallback, configureContainerCallback);

                Action<object> ConfigureContainerPipeline(Action<object> action)
                {
                    return Target;

                    // The ConfigureContainer pipeline needs an Action<TContainerBuilder> as source, so we just adapt the
                    // signature with this function.
                    void Source(TContainerBuilder containerBuilder) =>
                        action(containerBuilder);

                    // The ConfigureContainerBuilder.ConfigureContainerFilters expects an Action<object> as value, but our pipeline
                    // produces an Action<TContainerBuilder> given a source, so we wrap it on an Action<object> that internally casts
                    // the object containerBuilder to TContainerBuilder to match the expected signature of our ConfigureContainer pipeline.
                    void Target(object containerBuilder) =>
                        BuildStartupConfigureContainerFiltersPipeline(Source)((TContainerBuilder)containerBuilder);
                }
            }

            Func<IServiceCollection, IServiceProvider> ConfigureServices(
                Func<IServiceCollection, IServiceProvider> configureServicesCallback,
                Action<object> configureContainerCallback)
            {
                return ConfigureServicesWithContainerConfiguration;

                IServiceProvider ConfigureServicesWithContainerConfiguration(IServiceCollection services)
                {
                    // Call ConfigureServices, if that returned an IServiceProvider, we're done
                    var applicationServiceProvider = configureServicesCallback.Invoke(services);

                    if (applicationServiceProvider != null)
                    {
                        return applicationServiceProvider;
                    }

                    // If there's a ConfigureContainer method
                    if (ConfigureContainerBuilder.MethodInfo != null)
                    {
                        var serviceProviderFactory = HostingServiceProvider.GetRequiredService<IServiceProviderFactory<TContainerBuilder>>();
                        var builder = serviceProviderFactory.CreateBuilder(services);
                        configureContainerCallback(builder);
                        applicationServiceProvider = serviceProviderFactory.CreateServiceProvider(builder);
                    }
                    else
                    {
                        // Get the default factory
                        var serviceProviderFactory = HostingServiceProvider.GetRequiredService<IServiceProviderFactory<IServiceCollection>>();
                        var builder = serviceProviderFactory.CreateBuilder(services);
                        applicationServiceProvider = serviceProviderFactory.CreateServiceProvider(builder);
                    }

                    return applicationServiceProvider ?? services.BuildServiceProvider();
                }
            }

            private Func<IServiceCollection, IServiceProvider> BuildStartupServicesFilterPipeline(Func<IServiceCollection, IServiceProvider> startup)
            {
                return RunPipeline;

                IServiceProvider RunPipeline(IServiceCollection services)
                {
                    return startup(services);
                }
            }

            private Action<TContainerBuilder> BuildStartupConfigureContainerFiltersPipeline(Action<TContainerBuilder> configureContainer)
            {
                return RunPipeline;

                void RunPipeline(TContainerBuilder containerBuilder)
                {
                    Action<TContainerBuilder> pipeline = InvokeConfigureContainer;

                    pipeline(containerBuilder);

                    void InvokeConfigureContainer(TContainerBuilder builder) => configureContainer(builder);
                }
            }
        }
    }
}
