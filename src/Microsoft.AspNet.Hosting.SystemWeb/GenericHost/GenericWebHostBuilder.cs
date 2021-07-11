using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Web.Configuration;
using Microsoft.AspNet.Hosting.SystemWeb.Builder;
using Microsoft.AspNet.Hosting.SystemWeb.DependencyInjection;
using Microsoft.AspNet.Hosting.SystemWeb.Startup;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.ConfigurationManager;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Microsoft.AspNet.Hosting.SystemWeb
{
    /// <summary>
    /// A builder for <see cref="IWebHost"/>
    /// </summary>
    internal class GenericWebHostBuilder : IWebHostBuilder, ISupportsStartup, ISupportsUseDefaultServiceProvider
    {
        private readonly IHostBuilder builder;
        private readonly IConfiguration config;
        private readonly object startupKey = new object();
        private object startupObject;

        public GenericWebHostBuilder(IHostBuilder builder, GenericWebHostBuilderOptions options)
        {
            this.builder = builder;

            var configBuilder = new ConfigurationBuilder();

            if (!options.SuppressEnvironmentConfiguration)
            {
                configBuilder.AddEnvironmentVariables(prefix: "ASPNETCORE_");
                configBuilder.AddEnvironmentVariables(prefix: "ASPNET_");
            }

            if (!options.SuppressConfigurationConfiguration)
            {
                configBuilder.AddConfigurationManager(prefix: "aspnet:", skipConnectionStrings: true);
            }

            config = configBuilder.Build();

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

                // Add web object activator
                services.AddSingleton<IWebObjectActivator, WebObjectActivator>();

                services.Configure<GenericWebHostServiceOptions>(op =>
                {
                    // Set the options
                    op.WebHostOptions = webHostOptions;
                });

                services.TryAddSingleton<IApplicationBuilderFactory, ApplicationBuilderFactory>();

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

                        services.Configure<GenericWebHostServiceOptions>(op =>
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

        public IWebHost Build()
        {
            throw new NotSupportedException($"Building this implementation of {nameof(IWebHostBuilder)} is not supported.");
        }

        public IWebHostBuilder ConfigureAppConfiguration(Action<WebHostBuilderContext, IConfigurationBuilder> configureDelegate)
        {
            builder.ConfigureAppConfiguration((context, builder) =>
            {
                var webhostBuilderContext = GetWebHostBuilderContext(context);
                configureDelegate(webhostBuilderContext, builder);
            });

            return this;
        }

        public IWebHostBuilder ConfigureServices(Action<IServiceCollection> configureServices)
        {
            return ConfigureServices((context, services) => configureServices(services));
        }

        public IWebHostBuilder ConfigureServices(Action<WebHostBuilderContext, IServiceCollection> configureServices)
        {
            builder.ConfigureServices((context, builder) =>
            {
                var webhostBuilderContext = GetWebHostBuilderContext(context);
                configureServices(webhostBuilderContext, builder);
            });

            return this;
        }

        public IWebHostBuilder UseDefaultServiceProvider(Action<WebHostBuilderContext, ServiceProviderOptions> configure)
        {
            builder.UseServiceProviderFactory(context =>
            {
                var webHostBuilderContext = GetWebHostBuilderContext(context);
                var options = new ServiceProviderOptions();
                configure(webHostBuilderContext, options);
                return new DefaultServiceProviderFactory(options);
            });

            return this;
        }

        public IWebHostBuilder UseStartup(Type startupType)
        {
            // UseStartup can be called multiple times. Only run the last one.
            startupObject = startupType;

            builder.ConfigureServices((context, services) =>
            {
                // Run this delegate if the startup type matches
                if (ReferenceEquals(startupObject, startupType))
                {
                    UseStartup(startupType, context, services);
                }
            });

            return this;
        }

        public IWebHostBuilder UseStartup<TStartup>(Func<WebHostBuilderContext, TStartup> startupFactory)
        {
            // Clear the startup type
            startupObject = startupFactory;

            builder.ConfigureServices((context, services) =>
            {
                // UseStartup can be called multiple times. Only run the last one.
                if (ReferenceEquals(startupObject, startupFactory))
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
            return config[key];
        }

        public IWebHostBuilder UseSetting(string key, string value)
        {
            config[key] = value;
            return this;
        }

        public IWebHostBuilder Configure(Action<WebHostBuilderContext, IApplicationBuilder> configure)
        {
            // Clear the startup type
            this.startupObject = configure;

            this.builder.ConfigureServices((context, services) =>
            {
                if (object.ReferenceEquals(this.startupObject, configure))
                {
                    services.Configure<GenericWebHostServiceOptions>(options =>
                    {
                        var webhostBuilderContext = GetWebHostBuilderContext(context);
                        options.ConfigureApplication = app => configure(webhostBuilderContext, app);
                    });
                }
            });

            return this;
        }

        private static WebHostBuilderContext GetWebHostBuilderContext(HostBuilderContext context)
        {
            if (!context.Properties.TryGetValue(typeof(WebHostBuilderContext), out var contextVal))
            {
                var options = new WebHostOptions(context.Configuration, System.Web.Hosting.HostingEnvironment.SiteName ?? string.Empty);
                HostingEnvironment hostingEnvironment = new HostingEnvironment();
                var webHostBuilderContext = new WebHostBuilderContext
                {
                    Configuration = context.Configuration,
                    HostingEnvironment = hostingEnvironment,
                };
                hostingEnvironment.Initialize(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath, options);
                context.Properties[typeof(WebHostBuilderContext)] = webHostBuilderContext;
                context.Properties[typeof(WebHostOptions)] = options;
                return webHostBuilderContext;
            }

            // Refresh config, it's periodically updated/replaced
            var webHostContext = (WebHostBuilderContext)contextVal;
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
                if (typeof(IStartup).IsAssignableFrom(startupType))
                {
                    throw new NotSupportedException($"{typeof(IStartup)} isn't supported");
                }

                if (StartupLoader.HasConfigureServicesIServiceProviderDelegate(startupType, context.HostingEnvironment.EnvironmentName))
                {
                    throw new NotSupportedException($"ConfigureServices returning an {typeof(IServiceProvider)} isn't supported.");
                }

                if (instance is null)
                {
                    instance = ActivatorUtilities.CreateInstance(new HostServiceProvider(webHostBuilderContext), startupType);
                }

                context.Properties[startupKey] = instance;

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
                    builder.Properties[typeof(ConfigureContainerBuilder)] = configureContainerBuilder;

                    var actionType = typeof(Action<,>).MakeGenericType(typeof(HostBuilderContext), containerType);

                    // Get the private ConfigureContainer method on this type then close over the container type
                    var configureCallback = typeof(GenericWebHostBuilder).GetMethod(nameof(ConfigureContainerImpl), BindingFlags.NonPublic | BindingFlags.Instance)
                                                     .MakeGenericMethod(containerType)
                                                     .CreateDelegate(actionType, this);

                    // this.builder.ConfigureContainer<T>(ConfigureContainer);
                    typeof(IHostBuilder).GetMethod(nameof(IHostBuilder.ConfigureContainer))
                        .MakeGenericMethod(containerType)
                        .InvokeWithoutWrappingExceptions(builder, new object[] { configureCallback });
                }

                // Resolve Configure after calling ConfigureServices and ConfigureContainer
                configureBuilder = StartupLoader.FindConfigureDelegate(startupType, context.HostingEnvironment.EnvironmentName);
            }
            catch (Exception ex) when (webHostOptions.CaptureStartupErrors)
            {
                startupError = ExceptionDispatchInfo.Capture(ex);
            }

            // Startup.Configure
            services.Configure<GenericWebHostServiceOptions>(options =>
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
            var instance = context.Properties[startupKey];
            var builder = (ConfigureContainerBuilder)context.Properties[typeof(ConfigureContainerBuilder)];
            builder.Build(instance)(container);
        }

        // This exists just so that we can use ActivatorUtilities.CreateInstance on the Startup class
        private class HostServiceProvider : IServiceProvider
        {
            private readonly WebHostBuilderContext _context;

            public HostServiceProvider(WebHostBuilderContext context)
            {
                _context = context;
            }

            public object GetService(Type serviceType)
            {
                // The implementation of the HostingEnvironment supports both interfaces
#pragma warning disable CS0618 // Type or member is obsolete
                if (serviceType == typeof(Extensions.Hosting.IHostingEnvironment)
#pragma warning restore CS0618 // Type or member is obsolete
                    || serviceType == typeof(IWebHostEnvironment)
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
