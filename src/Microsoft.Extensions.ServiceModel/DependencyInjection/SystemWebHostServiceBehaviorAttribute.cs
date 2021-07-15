using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Microsoft.Extensions.ServiceModel.DependencyInjection
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public sealed class SystemWebHostServiceBehaviorAttribute : Attribute, IServiceBehavior
    {
        private readonly ServiceBehaviorAttribute serviceBehaviorAttribute;
        private readonly IServiceBehavior serviceBehavior;

        public SystemWebHostServiceBehaviorAttribute()
        {
            this.serviceBehaviorAttribute = new ServiceBehaviorAttribute();
            this.serviceBehavior = this.serviceBehaviorAttribute as IServiceBehavior;
        }

        [DefaultValue(null)]
        public string Name
        {
            get => this.serviceBehaviorAttribute.Name;
            set => this.serviceBehaviorAttribute.Name = value;
        }

        [DefaultValue(null)]
        public string Namespace
        {
            get => this.serviceBehaviorAttribute.Namespace;
            set => this.serviceBehaviorAttribute.Namespace = value;
        }

        [DefaultValue(AddressFilterMode.Exact)]
        public AddressFilterMode AddressFilterMode
        {
            get => this.serviceBehaviorAttribute.AddressFilterMode;
            set => this.serviceBehaviorAttribute.AddressFilterMode = value;
        }

        [DefaultValue(true)]
        public bool AutomaticSessionShutdown
        {
            get => this.serviceBehaviorAttribute.AutomaticSessionShutdown;
            set => this.serviceBehaviorAttribute.AutomaticSessionShutdown = value;
        }

        [DefaultValue(null)]
        public string ConfigurationName
        {
            get => this.serviceBehaviorAttribute.ConfigurationName;
            set => this.serviceBehaviorAttribute.ConfigurationName = value;
        }

        public System.Transactions.IsolationLevel TransactionIsolationLevel
        {
            get => this.serviceBehaviorAttribute.TransactionIsolationLevel;
            set => this.serviceBehaviorAttribute.TransactionIsolationLevel = value;
        }

        [DefaultValue(false)]
        public bool IncludeExceptionDetailInFaults
        {
            get => this.serviceBehaviorAttribute.IncludeExceptionDetailInFaults;
            set => this.serviceBehaviorAttribute.IncludeExceptionDetailInFaults = value;
        }

        [DefaultValue(ConcurrencyMode.Single)]
        public ConcurrencyMode ConcurrencyMode
        {
            get => this.serviceBehaviorAttribute.ConcurrencyMode;
            set => this.serviceBehaviorAttribute.ConcurrencyMode = value;
        }

        [DefaultValue(false)]
        public bool EnsureOrderedDispatch
        {
            get => this.serviceBehaviorAttribute.EnsureOrderedDispatch;
            set => this.serviceBehaviorAttribute.EnsureOrderedDispatch = value;
        }

        [DefaultValue(InstanceContextMode.PerSession)]
        public InstanceContextMode InstanceContextMode
        {
            get => this.serviceBehaviorAttribute.InstanceContextMode;
            set => this.serviceBehaviorAttribute.InstanceContextMode = value;
        }

        public bool ReleaseServiceInstanceOnTransactionComplete
        {
            get => this.serviceBehaviorAttribute.ReleaseServiceInstanceOnTransactionComplete;
            set => this.serviceBehaviorAttribute.ReleaseServiceInstanceOnTransactionComplete = value;
        }

        public bool TransactionAutoCompleteOnSessionClose
        {
            get => this.serviceBehaviorAttribute.TransactionAutoCompleteOnSessionClose;
            set => this.serviceBehaviorAttribute.TransactionAutoCompleteOnSessionClose = value;
        }

        public string TransactionTimeout
        {
            get => this.serviceBehaviorAttribute.TransactionTimeout;
            set => this.serviceBehaviorAttribute.TransactionTimeout = value;
        }

        [DefaultValue(true)]
        public bool ValidateMustUnderstand
        {
            get => this.serviceBehaviorAttribute.ValidateMustUnderstand;
            set => this.serviceBehaviorAttribute.ValidateMustUnderstand = value;
        }

        [DefaultValue(false)]
        public bool IgnoreExtensionDataObject
        {
            get => this.serviceBehaviorAttribute.IgnoreExtensionDataObject;
            set => this.serviceBehaviorAttribute.IgnoreExtensionDataObject = value;
        }

        [DefaultValue(int.MaxValue)]
        public int MaxItemsInObjectGraph
        {
            get => this.serviceBehaviorAttribute.MaxItemsInObjectGraph;
            set => this.serviceBehaviorAttribute.MaxItemsInObjectGraph = value;
        }

        [DefaultValue(true)]
        public bool UseSynchronizationContext
        {
            get => this.serviceBehaviorAttribute.UseSynchronizationContext;
            set => this.serviceBehaviorAttribute.UseSynchronizationContext = value;
        }

        void IServiceBehavior.Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            this.serviceBehavior.Validate(serviceDescription, serviceHostBase);
        }

        void IServiceBehavior.AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
            this.serviceBehavior.AddBindingParameters(serviceDescription, serviceHostBase, endpoints, bindingParameters);
        }

        void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (ChannelDispatcherBase cdb in serviceHostBase.ChannelDispatchers)
            {
                var dispatcher = cdb as ChannelDispatcher;
                foreach (EndpointDispatcher endpointDispatcher in dispatcher.Endpoints)
                {
                    if (!endpointDispatcher.IsSystemEndpoint)
                    {
                        endpointDispatcher.DispatchRuntime.InstanceProvider = SystemWebHostDependencyInjectionInstanceProvider.Shared;
                    }
                }
            }
        }
    }
}
