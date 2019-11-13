using System;
using System.Collections.Generic;

namespace Customizer.Texturing {
    /// <summary>
    /// Definition for composite texture
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
        Justification = "<Pending>")]
    public sealed class CoTextureDefinition : ICloneable {
        /// <summary>
        /// Preferred width of texture
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Preferred height of texture
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Component textures
        /// </summary>
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