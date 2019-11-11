using System;

namespace CustomNavi.Utility {
    /// <summary>
    /// Use custom serialization on members
    /// </summary>
    [AttributeUsageAttribute(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class CnCustomSerializeMembersAttribute : Attribute {
        /// <summary>
        /// Use custom serialization on members
        /// </summary>
        public CnCustomSerializeMembersAttribute() {
        }
    }
}