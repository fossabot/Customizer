using System;

namespace CustomNavi.Utility {
    /// <summary>
    /// Serialize member
    /// </summary>
    [AttributeUsageAttribute(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class CnSerializeAttribute : Attribute {
        /// <summary>
        /// Serialize member
        /// </summary>
        /// <param name="tag">Identifier tag</param>
        public CnSerializeAttribute(int tag) {
            Tag = tag;
        }

        /// <summary>
        /// Identifier tag
        /// </summary>
        public int Tag { get; }
    }
}