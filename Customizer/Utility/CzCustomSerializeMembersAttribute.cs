using System;

namespace Customizer.Utility {
    /// <summary>
    /// Use custom serialization on members
    /// </summary>
    [AttributeUsageAttribute(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class CzCustomSerializeMembersAttribute : Attribute {
        /// <summary>
        /// Use custom serialization on members
        /// </summary>
        public CzCustomSerializeMembersAttribute() {
        }
    }
}