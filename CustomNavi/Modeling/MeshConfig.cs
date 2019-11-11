using System;
using System.Collections.Generic;
using CustomNavi.Utility;

namespace CustomNavi.Modeling {
    /// <summary>
    /// Mesh configuration (import settings and custom properties)
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
        Justification = "<Pending>")]
    [CnCustomSerializeMembers]
    public class MeshConfig : ICloneable {
        /// <summary>
        /// Name of mesh this config is for
        /// </summary>
        [CnSerialize(0)]
        public string Mesh { get; set; }

        /// <summary>
        /// Parent attach point
        /// </summary>
        [CnSerialize(1)]
        public AttachPointType ParentAttachPoint { get; set; }

        /// <summary>
        /// Attach point local transform
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1819:PropertiesShouldNotReturnArrays", Justification = "<Pending>")]
        [CnSerialize(2)]
        public float[] AttachPointLocalTransform { get; set; } = new float[16];

        /// <summary>
        /// Custom attach points
        /// </summary>
        [CnSerialize(3)]
        public List<AttachPoint> CustomAttachPoints { get; set; } = new List<AttachPoint>();

        /// <summary>
        /// Materials used by mesh (ordered for base mesh)
        /// </summary>
        [CnSerialize(4)]
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