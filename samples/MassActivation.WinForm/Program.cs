using System;
using System.Windows.Forms;

namespace MassActivation.WinForm
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationActivator.UseApplicationName("MassActivation").UseApplicationVersion(new Version("1.0.5")).Startup();
        }
    }
}
