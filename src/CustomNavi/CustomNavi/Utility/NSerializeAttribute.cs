using System;

namespace CustomNavi.Utility {
    /// <summary>
    /// Serialize member
    /// </summary>
    [AttributeUsageAttribute(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class NSerializeAttribute : Attribute {
        public NSerializeAttribute(int tag) {
            Tag = tag;
        }

        /// <summary>
        /// Identifier tag
        /// </summary>
        public int Tag { get; }
    }
}