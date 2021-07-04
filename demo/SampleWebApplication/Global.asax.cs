using System;
using Microsoft.AspNet.Hosting;
using SampleWebApplication.App_Start;

namespace SampleWebApplication
{
    public class Global : global::System.Web.HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            SystemWebHosting.ConfigureHost(builder =>
            {
                // configure host here.
            });

            SystemWebHosting.ConfigureWebHost(builder =>
            {
                // configure web host here.

                builder.UseStartup<Startup>();
            });
        }
    }
}