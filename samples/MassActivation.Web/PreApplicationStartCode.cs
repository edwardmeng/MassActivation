using System;
using System.Web;
using MassActivation.Web;

[assembly:PreApplicationStartMethod(typeof(PreApplicationStartCode), "Startup")]
namespace MassActivation.Web
{
    public class PreApplicationStartCode
    {
        public static void Startup()
        {
            ApplicationActivator.UseApplicationName("MassActivation").UseApplicationVersion(new Version("1.0.5")).Startup();
        }
    }
}