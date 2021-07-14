using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Web;

namespace Microsoft.Extensions.ServiceModel.DependencyInjection
{
    internal sealed class SystemWebHostDependencyInjectionInstanceProvider : IInstanceProvider
    {
        public static readonly SystemWebHostDependencyInjectionInstanceProvider Shared = new SystemWebHostDependencyInjectionInstanceProvider();

        public object GetInstance(InstanceContext instanceContext)
        {
            return GetInstance(instanceContext, null);
        }

        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            return HttpRuntime.WebObjectActivator.GetService(instanceContext.Host.Description.ServiceType);
        }

        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
        }
    }
}
