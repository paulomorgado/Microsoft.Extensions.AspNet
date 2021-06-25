using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.AspNet.Hosting.HttpRuntime;
using Microsoft.AspNet.Hosting.HttpRuntime.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.HttpRuntime.Hosting;
using HostingEnvironment = Microsoft.Extensions.Hosting.Internal.HostingEnvironment;

namespace Microsoft.AspNet.Hosting
{
    /// <summary>
    /// A program initialization utility.
    /// </summary>
    public class HttpRuntimeHostBuilder : IHttpRuntimeHostBuilder, IHostBuilder
    {
        private List<Action<IConfigurationBuilder>> configureHostConfigActions = new List<Action<IConfigurationBuilder>>();
        private List<Action<HostBuilderContext, IConfigurationBuilder>> configureAppConfigActions = new List<Action<HostBuilderContext, IConfigurationBuilder>>();
        private List<Action<HostBuilderContext, IServiceCollection>> configureServicesActions = new List<Action<HostBuilderContext, IServiceCollection>>();
        private List<IConfigureContainerAdapter> configureContainerActions = new List<IConfigureContainerAdapter>();
        private IServiceFactoryAdapter serviceProviderFactory;
        private bool hostBuilt;
        private IConfiguration hostConfiguration;
        private IConfiguration appConfiguration;
        private HostBuilderContext hostBuilderContext;
        private HostingEnvironment hostingEnvironment;
        private IServiceProvider appServices;
        private PhysicalFileProvider defaultProvider;

        /// <summary>
        /// A central location for sharing state between components during the host building process.
        /// </summary>
        public IDictionary<object, object> Properties { get; } = new Dictionary<object, object>();

        public IHttpRuntimeHostBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate)
        {
            configureHostConfigActions.Add(configureDelegate ?? throw new ArgumentNullException(nameof(configureDelegate)));
            return this;
        }

        public IHttpRuntimeHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
        {
            configureAppConfigActions.Add(configureDelegate ?? throw new ArgumentNullException(nameof(configureDelegate)));
            return this;
        }

        public IHttpRuntimeHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            configureServicesActions.Add(configureDelegate ?? throw new ArgumentNullException(nameof(configureDelegate)));
            return this;
        }

        public IHttpRuntimeHostBuilder UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory)
        {
            serviceProviderFactory = new ServiceFactoryAdapter<TContainerBuilder>(factory ?? throw new ArgumentNullException(nameof(factory)));
            return this;
        }

        public IHttpRuntimeHostBuilder UseServiceProviderFactory<TContainerBuilder>(Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory)
        {
            serviceProviderFactory = new ServiceFactoryAdapter<TContainerBuilder>(() => hostBuilderContext, factory ?? throw new ArgumentNullException(nameof(factory)));
            return this;
        }

        public IHttpRuntimeHostBuilder ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate)
        {
            configureContainerActions.Add(new ConfigureContainerAdapter<TContainerBuilder>(configureDelegate
               ?? throw new ArgumentNullException(nameof(configureDelegate))));
            return this;
        }

        public IHttpRuntimeHost Build() => BuildImpl();

        IHostBuilder IHostBuilder.ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate) => ConfigureHostConfiguration(configureDelegate);

        IHostBuilder IHostBuilder.ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate) => ConfigureAppConfiguration(configureDelegate);

        IHostBuilder IHostBuilder.ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate) => ConfigureServices(configureDelegate);

        IHostBuilder IHostBuilder.UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory) => UseServiceProviderFactory(factory);

        IHostBuilder IHostBuilder.UseServiceProviderFactory<TContainerBuilder>(Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory) => UseServiceProviderFactory(factory);

        IHostBuilder IHostBuilder.ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate) => ConfigureContainer(configureDelegate);

        IHost IHostBuilder.Build() => BuildImpl();

        HttpRuntimeHostImpl BuildImpl()
        {
            if (hostBuilt)
            {
                throw new InvalidOperationException(@"Build can only be called once.");
            }

            hostBuilt = true;

            // REVIEW: If we want to raise more events outside of these calls then we will need to
            // stash this in a field.
            using (var diagnosticListener = new DiagnosticListener("Microsoft.AspNet.Hosting.HttpRuntime"))
            {
                const string hostBuildingEventName = "HostBuilding";
                const string hostBuiltEventName = "HostBuilt";

                Write(diagnosticListener, hostBuildingEventName, this);

                BuildHostConfiguration();
                CreateHostingEnvironment();
                CreateHostBuilderContext();
                BuildAppConfiguration();
                CreateServiceProvider();
                ConfigureApp();

                var host = appServices.GetRequiredService<HttpRuntimeHostImpl>();

                Write(diagnosticListener, hostBuiltEventName, host);

                return host;
            }
        }

        private static void Write<T>(DiagnosticListener diagnosticSource, string name, T value)
        {
            if (diagnosticSource.IsEnabled() && diagnosticSource.IsEnabled(name))
            {
                diagnosticSource.Write(name, value);
            }
        }

        private void BuildHostConfiguration()
        {
            IConfigurationBuilder configBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(); // Make sure there's some default storage since there are no default providers

            foreach (Action<IConfigurationBuilder> buildAction in configureHostConfigActions)
            {
                buildAction(configBuilder);
            }

            hostConfiguration = configBuilder.Build();
        }

        private void CreateHostingEnvironment()
        {
            hostingEnvironment = new HostingEnvironment()
            {
                ApplicationName = hostConfiguration[HostDefaults.ApplicationKey],
                EnvironmentName = hostConfiguration[HostDefaults.EnvironmentKey] ?? Environments.Production,
                ContentRootPath = ResolveContentRootPath(hostConfiguration[HostDefaults.ContentRootKey], AppContext.BaseDirectory),
            };

            if (string.IsNullOrEmpty(hostingEnvironment.ApplicationName))
            {
                // Note GetEntryAssembly returns null for the net4x console test runner.
                hostingEnvironment.ApplicationName = Assembly.GetEntryAssembly()?.GetName().Name;
            }

            hostingEnvironment.ContentRootFileProvider = defaultProvider = new PhysicalFileProvider(hostingEnvironment.ContentRootPath);
        }

        private string ResolveContentRootPath(string contentRootPath, string basePath)
        {
            if (string.IsNullOrEmpty(contentRootPath))
            {
                return basePath;
            }

            if (Path.IsPathRooted(contentRootPath))
            {
                return contentRootPath;
            }

            return Path.Combine(Path.GetFullPath(basePath), contentRootPath);
        }

        private void CreateHostBuilderContext()
        {
            hostBuilderContext = new HostBuilderContext(Properties)
            {
                HostingEnvironment = hostingEnvironment,
                Configuration = hostConfiguration
            };
        }

        private void BuildAppConfiguration()
        {
            IConfigurationBuilder configBuilder = new ConfigurationBuilder()
                .SetBasePath(hostingEnvironment.ContentRootPath)
                .AddConfiguration(hostConfiguration, shouldDisposeConfiguration: true);

            foreach (Action<HostBuilderContext, IConfigurationBuilder> buildAction in configureAppConfigActions)
            {
                buildAction(hostBuilderContext, configBuilder);
            }
            appConfiguration = configBuilder.Build();
            hostBuilderContext.Configuration = appConfiguration;
        }

        private void CreateServiceProvider()
        {
            var services = new ServiceCollection();
#pragma warning disable CS0618 // Type or member is obsolete
            services.AddSingleton<IHostingEnvironment>(hostingEnvironment);
#pragma warning restore CS0618 // Type or member is obsolete
            services.AddSingleton<IHostEnvironment>(hostingEnvironment);
            services.AddSingleton(hostBuilderContext);
            // register configuration as factory to make it dispose with the service provider
            services.AddSingleton(_ => appConfiguration);
            services.AddSingleton<IHostLifetime, HttpRuntimeHostLifetime>();
            services.AddSingleton<IHostApplicationLifetime, HttpRuntimeHostApplicationLifetime>();
#pragma warning disable CS0618 // Type or member is obsolete
            services.AddSingleton<IApplicationLifetime>(s => (IApplicationLifetime)s.GetService<IHostApplicationLifetime>());
#pragma warning restore CS0618 // Type or member is obsolete
            services.AddSingleton<HttpRuntimeHostImpl>(_ =>
            {
                return new HttpRuntimeHostImpl(appServices,
                    hostingEnvironment,
                    defaultProvider,
                    appServices.GetRequiredService<IHostApplicationLifetime>(),
                    appServices.GetRequiredService<ILogger<HttpRuntimeHostImpl>>(),
                    appServices.GetRequiredService<IHostLifetime>(),
                    appServices.GetRequiredService<IOptions<HostOptions>>());
            });
            services.AddSingleton<IHost>(sp => sp.GetRequiredService<HttpRuntimeHostImpl>());
            services.AddSingleton<IHttpRuntimeHost>(sp => sp.GetRequiredService<HttpRuntimeHostImpl>());
            services.AddOptions().Configure<HostOptions>(options => { });
            services.AddLogging();

            foreach (Action<HostBuilderContext, IServiceCollection> configureServicesAction in configureServicesActions)
            {
                configureServicesAction(hostBuilderContext, services);
            }

            if (serviceProviderFactory is null)
            {
                serviceProviderFactory = new ServiceFactoryAdapter<IServiceCollection>(new HttpRuntimeServiceProviderFactory());
            }

            object containerBuilder = serviceProviderFactory.CreateBuilder(services);

            foreach (IConfigureContainerAdapter containerAction in configureContainerActions)
            {
                containerAction.ConfigureContainer(hostBuilderContext, containerBuilder);
            }

            appServices = serviceProviderFactory.CreateServiceProvider(containerBuilder);

            if (appServices == null)
            {
                throw new InvalidOperationException(@"The IServiceProviderFactory returned a null IServiceProvider");
            }

            // resolve configuration explicitly once to mark it as resolved within the
            // service provider, ensuring it will be properly disposed with the provider
            _ = appServices.GetService<IConfiguration>();
        }

        private void ConfigureApp()
        {
            var options = appServices.GetRequiredService<IOptions<HttpRuntimeWebHostServiceOptions>>().Value;
            var applicationBuilder = appServices.GetRequiredService<IHttpRuntimeApplicationBuilder>();

            options.ConfigureApplication(applicationBuilder);
        }
    }
}
