using System;
using System.Collections.Generic;
using Customizer.Utility;

namespace Customizer.Modeling {
    /// <summary>
    /// Mesh configuration (import settings and custom properties)
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
        Justification = "<Pending>")]
    [CzCustomSerializeMembers]
    public sealed class MeshConfig : ICloneable {
        /// <summary>
        /// Name of mesh this config is for
        /// </summary>
        [CzSerialize(0)]
        public string Mesh { get; set; }

        /// <summary>
        /// Parent attach point
        /// </summary>
        [CzSerialize(1)]
        public AttachPointType ParentAttachPoint { get; set; }

        /// <summary>
        /// Attach point local transform
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1819:PropertiesShouldNotReturnArrays", Justification = "<Pending>")]
        [CzSerialize(2)]
        public float[] AttachPointLocalTransform { get; set; } = new float[16];

        /// <summary>
        /// Custom attach points
        /// </summary>
        [CzSerialize(3)]
        public List<AttachPoint> CustomAttachPoints { get; set; } = new List<AttachPoint>();

        /// <summary>
        /// Materials used by mesh (ordered for base mesh)
        /// </summary>
        [CzSerialize(4)]
        public List<Material> Materials { get; set; } = new List<Material>();


        public object Clone() {
            var res = new MeshConfig {
                Mesh = Mesh,
                ParentAttachPoint = ParentAttachPoint
            };
            res.CustomAttachPoints.AddRange(CustomAttachPoints);
            Array.Copy(AttachPointLocalTransform, res.AttachPointLocalTransform, AttachPointLocalTransform.Length);
            res.Materials.AddRange(Materials);
            return res;
        }
    }
}