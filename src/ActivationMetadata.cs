using System;
using System.Reflection;

namespace MassActivation
{
    internal class ActivationMetadata
    {
        private ActivationPriority? _priority;
        private readonly ActivationPriority _defaultPriority;

        public ActivationMetadata(MemberInfo targetMember, ActivationPriority defaultPriority = ActivationPriority.Normal)
        {
            TargetMember = targetMember;
            _defaultPriority = defaultPriority;
        }

        public MemberInfo TargetMember { get; }

        public object TargetInstance { get; set; }

        public ActivationPriority Priority
        {
            get
            {
                if (!_priority.HasValue)
                {
#if NetCore
                    var attribute = TargetMember.GetCustomAttribute<ActivationPriorityAttribute>();
#else
                    var attribute = (ActivationPriorityAttribute)Attribute.GetCustomAttribute(TargetMember,typeof(ActivationPriorityAttribute));
#endif
                    _priority = attribute?.Priority ?? _defaultPriority;
                }
                return _priority.Value;
            }
        }

        public Type GetTargetType()
        {
            switch (TargetMember.MemberType)
            {
                case MemberTypes.Constructor:
                case MemberTypes.Event:
                case MemberTypes.Field:
                case MemberTypes.Method:
                case MemberTypes.Property:
                    return TargetMember.DeclaringType;
                case MemberTypes.TypeInfo:
                case MemberTypes.NestedType:
#if NetCore
                    return ((TypeInfo) TargetMember).AsType();
#else
                    return (Type) TargetMember;
#endif
            }
            return null;
        }

        public Assembly GetTargetAssembly()
        {
            var type = GetTargetType();
            if (type == null) return null;
#if NetCore
            return type.GetTypeInfo().Assembly;
#else
            return type.Assembly;
#endif
        }
    }
}
