using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Microsoft.Extensions.ServiceModel.DependencyInjection
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public sealed class SystemWebHostDependencyInjectionAttribute : Attribute, IServiceBehavior
    {
        void IServiceBehavior.Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            var myIndex = serviceDescription.Behaviors.IndexOf(this);
            if (myIndex != (serviceDescription.Behaviors.Count - 1))
            {
                // Move this behavior to the end.
                serviceDescription.Behaviors.RemoveAt(myIndex);
                serviceDescription.Behaviors.Add(this);

                // As we've shifted everything down, the behavior at serviceDescription.Behaviors[myIndex] won't have Validate called. So, do that.
                serviceDescription.Behaviors[myIndex].Validate(serviceDescription, serviceHostBase);
            }
        }

        void IServiceBehavior.AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
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
