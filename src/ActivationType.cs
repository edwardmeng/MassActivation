using System.Collections.Generic;
using System.Linq;

namespace MassActivation
{
    internal class ActivationType
    {
        private readonly HashSet<string> _invokingMethodNames = new HashSet<string>();
        private readonly HashSet<string> _invokedMethodNames = new HashSet<string>();

        public ActivationType(ActivationMetadata metadata)
        {
            Metadata = metadata;
        }

        public ActivationMetadata Metadata { get; }

        public void EnqueueMethods(IEnumerable<string> methodNames)
        {
            foreach (var methodName in methodNames)
            {
                if (!_invokedMethodNames.Contains(methodName))
                {
                    _invokingMethodNames.Add(methodName);
                }
            }
        }

        public string PeekMethod()
        {
            return _invokingMethodNames.FirstOrDefault();
        }

        public void CommitMethod(string methodName)
        {
            _invokingMethodNames.Remove(methodName);
            _invokedMethodNames.Add(methodName);
        }
    }
}
