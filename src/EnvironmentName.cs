namespace Wheatech.Hosting
{
    /// <summary>
    /// Commonly used environment names. 
    /// </summary>
    public static class EnvironmentName
    {
        /// <summary>
        /// The development environment used by <see cref="AppHost"/>.
        /// </summary>
        public static readonly string Development = "Development";

        /// <summary>
        /// The production environment used by <see cref="AppHost"/>.
        /// </summary>
        public static readonly string Production = "Production";

        /// <summary>
        /// The staging environment used by <see cref="AppHost"/>.
        /// </summary>
        public static readonly string Staging = "Staging";
    }
}
