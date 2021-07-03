using Microsoft.AspNet.Hosting;
using SampleWebApplication.App_Start;

namespace SampleWebApplication
{
    public class Global : HttpRuntimeHostHttpApplication<Startup>
    {
    }
}