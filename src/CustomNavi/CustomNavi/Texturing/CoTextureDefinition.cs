using System;
using System.Collections.Generic;

namespace CustomNavi.Texturing {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
        Justification = "<Pending>")]
    public class CoTextureDefinition : ICloneable {
        public int Width { get; set; }
        public int Height { get; set; }
        public List<SubTextureDefinition> Textures { get; set; } = new List<SubTextureDefinition>();

        public object Clone() {
            var res = new CoTextureDefinition {
                Width = Width,
                Height = Height
            };
            foreach (var e in Textures)
                res.Textures.Add(e);
            return res;
        }
    }
}