using System;

namespace Customizer.Utility {
    /// <summary>
    /// Serialize member
    /// </summary>
    [AttributeUsageAttribute(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class CzSerializeAttribute : Attribute {
        /// <summary>
        /// Serialize member
        /// </summary>
        /// <param name="tag">Identifier tag</param>
        public CzSerializeAttribute(int tag) {
            Tag = tag;
        }

        /// <summary>
        /// Identifier tag
        /// </summary>
        public int Tag { get; }
    }
}