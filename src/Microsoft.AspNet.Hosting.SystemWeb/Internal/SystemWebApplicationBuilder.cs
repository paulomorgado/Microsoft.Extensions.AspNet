using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Microsoft.AspNet.Hosting.SystemWeb
{
    internal sealed class SystemWebApplicationBuilder : IApplicationBuilder
    {
        private const string ServerFeaturesKey = "server.Features";
        private const string ApplicationServicesKey = "application.Services";

        private readonly IList<Func<RequestDelegate, RequestDelegate>> components = new List<Func<RequestDelegate, RequestDelegate>>();

        public SystemWebApplicationBuilder(IServiceProvider serviceProvider)
        {
            Properties = new Dictionary<string, object>(StringComparer.Ordinal);
            ApplicationServices = serviceProvider;
        }

        public SystemWebApplicationBuilder(IServiceProvider serviceProvider, object server)
            : this(serviceProvider)
        {
            SetProperty(ServerFeaturesKey, server);
        }

        private SystemWebApplicationBuilder(SystemWebApplicationBuilder builder)
        {
            Properties = new Dictionary<string, object>(builder.Properties, StringComparer.Ordinal);
        }

        public IServiceProvider ApplicationServices
        {
            get => GetProperty<IServiceProvider>(ApplicationServicesKey);
            set => SetProperty(ApplicationServicesKey, value);
        }

        public IFeatureCollection ServerFeatures => GetProperty<IFeatureCollection>(ServerFeaturesKey);

        public IDictionary<string, object> Properties { get; }

        private T GetProperty<T>(string key) => Properties.TryGetValue(key, out var value) ? (T)value : default;

        private void SetProperty<T>(string key, T value) => Properties[key] = value;

        public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware)
        {
            components.Add(middleware);
            return this;
        }

        public IApplicationBuilder New() => new SystemWebApplicationBuilder(this);

        public RequestDelegate Build() => throw new InvalidOperationException();
    }
}
