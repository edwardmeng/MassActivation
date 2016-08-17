using System;

namespace MassActivation.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            ApplicationActivator.UseApplicationName("MassActivation").UseApplicationVersion(new Version("1.0.5")).Startup();
            System.Console.WriteLine("Press any key to continue");
            System.Console.Read();
        }
    }
}
