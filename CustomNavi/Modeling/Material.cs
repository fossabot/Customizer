using System;
using System.Collections.Generic;
using CustomNavi.Utility;

namespace CustomNavi.Modeling {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
        Justification = "<Pending>")]
    [CnCustomSerializeMembers]
    public class Material : ICloneable {
        [CnSerialize(0)] public Dictionary<string, string> CoTextures { get; set; } = new Dictionary<string, string>();

        public object Clone() {
            var res = new Material();
            foreach (var e in CoTextures)
                res.CoTextures.Add(e.Key, e.Value);
            return res;
        }
    }
}