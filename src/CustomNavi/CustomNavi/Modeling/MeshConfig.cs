using System;
using System.Collections.Generic;
using CustomNavi.Utility;

namespace CustomNavi.Modeling {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
        Justification = "<Pending>")]
    [NCustomSerializeMembers]
    public class MeshConfig : ICloneable {
        [NSerialize(0)] public int MeshIdx { get; set; }
        [NSerialize(1)] public AttachPointType ParentAttachPoint { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1819:PropertiesShouldNotReturnArrays", Justification = "<Pending>")]
        [NSerialize(2)]
        public float[] AttachPointLocalTransform { get; set; } = new float[16];

        [NSerialize(3)]
        public Dictionary<int, AttachPoint> CustomAttachPoints { get; set; } = new Dictionary<int, AttachPoint>();
        [NSerialize(4)]
        public List<Material> Materials { get; set; } = new List<Material>();
        

        public object Clone() {
            var res = new MeshConfig {
                MeshIdx = MeshIdx,
                ParentAttachPoint = ParentAttachPoint
            };
            foreach (var e in CustomAttachPoints)
                res.CustomAttachPoints.Add(e.Key, e.Value);
            Array.Copy(AttachPointLocalTransform, res.AttachPointLocalTransform, AttachPointLocalTransform.Length);
            res.Materials.AddRange(Materials);
            return res;
        }
    }
}