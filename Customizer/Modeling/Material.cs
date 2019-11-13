using System;
using System.Collections.Generic;
using Customizer.Utility;

namespace Customizer.Modeling {
    /// <summary>
    /// Material information
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
        Justification = "<Pending>")]
    [CzCustomSerializeMembers]
    public sealed class Material : ICloneable {
        /// <summary>
        /// Material texture properties
        /// </summary>
        [CzSerialize(0)]
        public Dictionary<string, string> Textures { get; set; } = new Dictionary<string, string>();

        public object Clone() {
            var res = new Material();
            foreach (var e in Textures)
                res.Textures.Add(e.Key, e.Value);
            return res;
        }
    }
}