using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Web.Compilation;
using System.Web.Configuration;
using Microsoft.AspNet.Hosting.HttpRuntime;
using Microsoft.AspNet.Hosting.HttpRuntime.DependencyInjection;
using Microsoft.AspNet.Hosting.HttpRuntime.Startup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.HttpRuntime.Hosting;

namespace Microsoft.AspNet.Hosting
{
    /// <summary>
    /// A builder for <see cref="IHttpRuntimeWebHost"/>
    /// </summary>
    internal class HttpRuntimeWebHostBuilder : IHttpRuntimeWebHostBuilder, ISupportsStartup, ISupportsUseDefaultServiceProvider
    {
        private readonly IHostBuilder builder;
        private readonly IConfiguration config;
        private readonly object startupKey = new object();
        private object startupObject;

        public HttpRuntimeWebHostBuilder(IHostBuilder builder, HttpRuntimeWebHostBuilderOptions options)
        {
            this.builder = builder;

            var configBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(GetAppSettings(options.SuppressEnvironmentConfiguration));

            if (!options.SuppressEnvironmentConfiguration)
            {
                configBuilder.AddEnvironmentVariables(prefix: "ASPNETCORE_");
                configBuilder.AddEnvironmentVariables(prefix: "ASPNET_");
            }

            this.config = configBuilder.Build();

            this.builder.ConfigureHostConfiguration(config =>
            {
                config.AddConfiguration(this.config);
            });

            this.builder.ConfigureAppConfiguration((context, configurationBuilder) =>
            {
            });

            this.builder.ConfigureServices((context, services) =>
            {
                var webhostContext = GetWebHostBuilderContext(context);
                var webHostOptions = (WebHostOptions)context.Properties[typeof(WebHostOptions)];

                // Add the IHostingEnvironment and IApplicationLifetime from Microsoft.AspNetCore.Hosting
                services.AddSingleton(webhostContext.HostingEnvironment);

                services.Configure<HttpRuntimeWebHostServiceOptions>(op =>
                {
                    // Set the options
                    op.WebHostOptions = webHostOptions;
                });

                services.TryAddSingleton<IHttpRuntimeApplicationBuilder, HttpRuntimeApplicationBuilder>();

                // Support UseStartup(assemblyName)
                if (!string.IsNullOrEmpty(webHostOptions.StartupAssembly))
                {
                    try
                    {
                        var startupType = StartupLoader.FindStartupType(webHostOptions.StartupAssembly, webhostContext.HostingEnvironment.EnvironmentName);
                        UseStartup(startupType, context, services);
                    }
                    catch (Exception ex) when (webHostOptions.CaptureStartupErrors)
                    {
                        var capture = ExceptionDispatchInfo.Capture(ex);

                        services.Configure<HttpRuntimeWebHostServiceOptions>(op =>
                        {
                            op.ConfigureApplication = app =>
                            {
                                // Throw if there was any errors initializing startup
                                capture.Throw();
                            };
                        });
                    }
                }
            });
        }

        public IHttpRuntimeWebHost Build()
        {
            throw new NotSupportedException($"Building this implementation of {nameof(IHttpRuntimeWebHostBuilder)} is not supported.");
        }

        public IHttpRuntimeWebHostBuilder ConfigureAppConfiguration(Action<HttpRuntimeWebHostBuilderContext, IConfigurationBuilder> configureDelegate)
        {
            this.builder.ConfigureAppConfiguration((context, builder) =>
            {
                var webhostBuilderContext = GetWebHostBuilderContext(context);
                configureDelegate(webhostBuilderContext, builder);
            });

            return this;
        }

        public IHttpRuntimeWebHostBuilder ConfigureServices(Action<IServiceCollection> configureServices)
        {
            return ConfigureServices((context, services) => configureServices(services));
        }

        public IHttpRuntimeWebHostBuilder ConfigureServices(Action<HttpRuntimeWebHostBuilderContext, IServiceCollection> configureServices)
        {
            this.builder.ConfigureServices((context, builder) =>
            {
                var webhostBuilderContext = GetWebHostBuilderContext(context);
                configureServices(webhostBuilderContext, builder);
            });

            return this;
        }

        public IHttpRuntimeWebHostBuilder UseDefaultServiceProvider(Action<HttpRuntimeWebHostBuilderContext, ServiceProviderOptions> configure)
        {
            this.builder.UseServiceProviderFactory(context =>
            {
                var webHostBuilderContext = GetWebHostBuilderContext(context);
                var options = new ServiceProviderOptions();
                configure(webHostBuilderContext, options);
                return new DefaultServiceProviderFactory(options);
            });

            return this;
        }

        public IHttpRuntimeWebHostBuilder UseStartup(Type startupType)
        {
            // UseStartup can be called multiple times. Only run the last one.
            this.startupObject = startupType;

            this.builder.ConfigureServices((context, services) =>
            {
                // Run this delegate if the startup type matches
                if (object.ReferenceEquals(this.startupObject, startupType))
                {
                    UseStartup(startupType, context, services);
                }
            });

            return this;
        }

        public IHttpRuntimeWebHostBuilder UseStartup<TStartup>(Func<HttpRuntimeWebHostBuilderContext, TStartup> startupFactory)
        {
            // Clear the startup type
            this.startupObject = startupFactory;

            this.builder.ConfigureServices((context, services) =>
            {
                // UseStartup can be called multiple times. Only run the last one.
                if (object.ReferenceEquals(this.startupObject, startupFactory))
                {
                    var webHostBuilderContext = GetWebHostBuilderContext(context);

                    var instance = startupFactory(webHostBuilderContext);

                    if (instance == null)
                    {
                        throw new InvalidOperationException("The specified factory returned null startup instance.");
                    }

                    UseStartup(instance.GetType(), context, services, instance);
                }
            });

            return this;
        }

        public string GetSetting(string key)
        {
            return this.config[key];
        }

        public IHttpRuntimeWebHostBuilder UseSetting(string key, string value)
        {
            this.config[key] = value;
            return this;
        }

        public IHttpRuntimeWebHostBuilder Configure(Action<HttpRuntimeWebHostBuilderContext, IHttpRuntimeApplicationBuilder> configure)
        {
            throw new NotImplementedException();
        }

        private static IEnumerable<KeyValuePair<string, string>> GetAppSettings(bool suppressEnvironmentConfiguration)
        {
            if (suppressEnvironmentConfiguration)
            {
                return Enumerable.Empty<KeyValuePair<string, string>>();
            }

            var appSettings = WebConfigurationManager.AppSettings;

            if (appSettings.Count == 0)
            {
                return Enumerable.Empty<KeyValuePair<string, string>>();
            }

            return GetEnumerable(appSettings);

            IEnumerable<KeyValuePair<string, string>> GetEnumerable(System.Collections.Specialized.NameValueCollection settings)
            {
                foreach (var key in settings.AllKeys)
                {
                    if (key.StartsWith("Microsoft:AspNet:Hosting:HttpRuntime:", StringComparison.OrdinalIgnoreCase)
                        && settings.GetValues(key) is string[] values && values.Length > 0)
                    {
                        yield return new KeyValuePair<string, string>(key.Substring(37), values[values.Length - 1]);
                    }
                }

            }
        }

        private static HttpRuntimeWebHostBuilderContext GetWebHostBuilderContext(HostBuilderContext context)
        {
            if (!context.Properties.TryGetValue(typeof(HttpRuntimeWebHostBuilderContext), out var contextVal))
            {
                var options = new WebHostOptions(context.Configuration, BuildManager.GetGlobalAsaxType()?.BaseType.Assembly.GetName().Name ?? string.Empty);
                var webHostBuilderContext = new HttpRuntimeWebHostBuilderContext
                {
                    Configuration = context.Configuration,
                    HostingEnvironment = new HttpRuntimeHostingEnvironment(),
                };
                webHostBuilderContext.HostingEnvironment.Initialize(System.Web.HttpRuntime.AppDomainAppPath, options);
                context.Properties[typeof(HttpRuntimeWebHostBuilderContext)] = webHostBuilderContext;
                context.Properties[typeof(WebHostOptions)] = options;
                return webHostBuilderContext;
            }

            // Refresh config, it's periodically updated/replaced
            var webHostContext = (HttpRuntimeWebHostBuilderContext)contextVal;
            webHostContext.Configuration = context.Configuration;
            return webHostContext;
        }

        private void UseStartup(Type startupType, HostBuilderContext context, IServiceCollection services, object instance = null)
        {
            var webHostBuilderContext = GetWebHostBuilderContext(context);
            var webHostOptions = (WebHostOptions)context.Properties[typeof(WebHostOptions)];

            ExceptionDispatchInfo startupError = null;
            ConfigureBuilder configureBuilder = null;

            try
            {
                // We cannot support methods that return IServiceProvider as that is terminal and we need ConfigureServices to compose
                if (typeof(IHttpRuntimeStartup).IsAssignableFrom(startupType))
                {
                    throw new NotSupportedException($"{typeof(IHttpRuntimeStartup)} isn't supported");
                }

                if (StartupLoader.HasConfigureServicesIServiceProviderDelegate(startupType, context.HostingEnvironment.EnvironmentName))
                {
                    throw new NotSupportedException($"ConfigureServices returning an {typeof(IServiceProvider)} isn't supported.");
                }

                if (instance is null)
                {
                    instance = ActivatorUtilities.CreateInstance(new HostServiceProvider(webHostBuilderContext), startupType);
                }

                context.Properties[this.startupKey] = instance;

                // Startup.ConfigureServices
                var configureServicesBuilder = StartupLoader.FindConfigureServicesDelegate(startupType, context.HostingEnvironment.EnvironmentName);
                var configureServices = configureServicesBuilder.Build(instance);

                configureServices(services);

                // REVIEW: We're doing this in the callback so that we have access to the hosting environment
                // Startup.ConfigureContainer
                var configureContainerBuilder = StartupLoader.FindConfigureContainerDelegate(startupType, context.HostingEnvironment.EnvironmentName);
                if (configureContainerBuilder.MethodInfo != null)
                {
                    var containerType = configureContainerBuilder.GetContainerType();
                    // Store the builder in the property bag
                    this.builder.Properties[typeof(ConfigureContainerBuilder)] = configureContainerBuilder;

                    var actionType = typeof(Action<,>).MakeGenericType(typeof(HostBuilderContext), containerType);

                    // Get the private ConfigureContainer method on this type then close over the container type
                    var configureCallback = typeof(HttpRuntimeWebHostBuilder).GetMethod(nameof(ConfigureContainerImpl), BindingFlags.NonPublic | BindingFlags.Instance)
                                                     .MakeGenericMethod(containerType)
                                                     .CreateDelegate(actionType, this);

                    // this.builder.ConfigureContainer<T>(ConfigureContainer);
                    typeof(IHostBuilder).GetMethod(nameof(IHostBuilder.ConfigureContainer))
                        .MakeGenericMethod(containerType)
                        .InvokeWithoutWrappingExceptions(this.builder, new object[] { configureCallback });
                }

                // Resolve Configure after calling ConfigureServices and ConfigureContainer
                configureBuilder = StartupLoader.FindConfigureDelegate(startupType, context.HostingEnvironment.EnvironmentName);
            }
            catch (Exception ex) when (webHostOptions.CaptureStartupErrors)
            {
                startupError = ExceptionDispatchInfo.Capture(ex);
            }

            // Startup.Configure
            services.Configure<HttpRuntimeWebHostServiceOptions>(options =>
            {
                options.ConfigureApplication = app =>
                {
                    // Throw if there was any errors initializing startup
                    startupError?.Throw();

                    // Execute Startup.Configure
                    if (instance != null && configureBuilder != null)
                    {
                        configureBuilder.Build(instance)(app);
                    }
                };
            });
        }

        private void ConfigureContainerImpl<TContainer>(HostBuilderContext context, TContainer container) where TContainer : class
        {
            var instance = context.Properties[this.startupKey];
            var builder = (ConfigureContainerBuilder)context.Properties[typeof(ConfigureContainerBuilder)];
            builder.Build(instance)(container);
        }

        // This exists just so that we can use ActivatorUtilities.CreateInstance on the Startup class
        private class HostServiceProvider : IServiceProvider
        {
            private readonly HttpRuntimeWebHostBuilderContext _context;

            public HostServiceProvider(HttpRuntimeWebHostBuilderContext context)
            {
                _context = context;
            }

            public object GetService(Type serviceType)
            {
                // The implementation of the HostingEnvironment supports both interfaces
#pragma warning disable CS0618 // Type or member is obsolete
                if (serviceType == typeof(Microsoft.Extensions.Hosting.IHostingEnvironment)
#pragma warning restore CS0618 // Type or member is obsolete
                    || serviceType == typeof(IHttpRuntimeWebHostEnvironment)
                    || serviceType == typeof(IHostEnvironment)
                    )
                {
                    return _context.HostingEnvironment;
                }

                if (serviceType == typeof(IConfiguration))
                {
                    return _context.Configuration;
                }

                return null;
            }
        }
    }
}
