using System;

namespace Wheatech.Activation
{
    /// <summary>
    /// Used to mark the priority of the activation class, constructor or methods.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Constructor)]
    public class ActivationPriorityAttribute : Attribute
    {
        /// <summary>
        /// Initialize new instance of <see cref="ActivationPriorityAttribute"/> with specified priority.
        /// </summary>
        /// <param name="priority">The priority of the target activation.</param>
        public ActivationPriorityAttribute(ActivationPriority priority)
        {
            Priority = priority;
        }

        /// <summary>
        /// Gets the priority of the target activation.
        /// </summary>
        public ActivationPriority Priority { get; }
    }
}
