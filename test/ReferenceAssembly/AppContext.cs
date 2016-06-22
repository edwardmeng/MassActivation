namespace ReferenceAssembly
{
    public class AppContext : IAppContext
    {
        public string UserName
        {
            get { return AssemblyEnvironment.UserName; }
            set { AssemblyEnvironment.UserName = value; }
        }
    }
}
