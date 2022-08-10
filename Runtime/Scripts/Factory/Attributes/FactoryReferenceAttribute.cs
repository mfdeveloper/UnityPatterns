using System;

namespace UnityPatterns.Factory.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = true, AllowMultiple = false)]
    public sealed class FactoryReferenceAttribute : Attribute
    {
        public FactoryReferenceAttribute()
        {
        }
    }
}
