namespace MassActivation
{
    /// <summary>
    /// The enumeration of the priority for the activation class, constructor or methods.
    /// </summary>
    public enum ActivationPriority
    {
        /// <summary>
        /// The highest priority to be executed at first level.
        /// </summary>
        Highest = 0,

        /// <summary>
        /// The high priority to be executed at the second level.
        /// </summary>
        High = 1,

        /// <summary>
        /// The normal priority to be executed at the middle level.
        /// </summary>
        Normal = 2,

        /// <summary>
        /// The low priority to be executed at the last second level.
        /// </summary>
        Low = 3,

        /// <summary>
        /// The lowest priority to be executed at the last level.
        /// </summary>
        Lowest = 4
    }
}
