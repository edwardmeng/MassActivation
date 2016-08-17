using System.Windows.Forms;
using MassActivation.Services;

namespace MassActivation.WinForm
{
    public class Startup
    {
        public void Configuration(IActivatingEnvironment environment, ICacheService cache)
        {
            cache.Set("ApplicationName", environment.ApplicationName);
            cache.Set("ApplicationVersion", environment.ApplicationVersion);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(cache));
        }
    }
}
