using System;
using System.Collections.Generic;
using CustomNavi.Utility;

namespace CustomNavi.Modeling {
    /// <summary>
    /// Material information
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
        Justification = "<Pending>")]
    [CnCustomSerializeMembers]
    public class Material : ICloneable {
        /// <summary>
        /// Material texture properties
        /// </summary>
        [CnSerialize(0)]
        public Dictionary<string, string> Textures { get; set; } = new Dictionary<string, string>();

        public object Clone() {
            var res = new Material();
            foreach (var e in Textures)
                res.Textures.Add(e.Key, e.Value);
            return res;
        }
    }
}