using System.Web;

namespace Microsoft.AspNet.Hosting.SystemWeb
{
    public interface IPipelineManager
    {
        void Register(HttpApplication httpApplication);
    }
}
