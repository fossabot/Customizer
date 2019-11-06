using System;
using System.Collections.Generic;
using CustomNavi.Utility;

namespace CustomNavi.Modeling {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
        Justification = "<Pending>")]
    [NCustomSerializeMembers]
    public class Material : ICloneable {
        [NSerialize(0)] public Dictionary<string, int> CoTextures { get; set; } = new Dictionary<string, int>();

        public object Clone() {
            var res = new Material();
            foreach (var e in CoTextures)
                res.CoTextures.Add(e.Key, e.Value);
            return res;
        }
    }
}