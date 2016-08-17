using System;
using System.Windows;

namespace MassActivation.WPF
{
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            var app = new App();
            app.InitializeComponent();

            ApplicationActivator
            .UseService<Application>(app)
            .UseApplicationName("MassActivation")
            .UseApplicationVersion(new Version("1.0.5"))
            .Startup();

            app.Run();
        }
    }
}
