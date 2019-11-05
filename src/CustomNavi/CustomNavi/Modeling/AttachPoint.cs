using CustomNavi.Utility;

namespace CustomNavi.Modeling {
    [NCustomSerializeMembers]
    public class AttachPoint {
        [NSerialize(0)] public AttachPointType Type { get; set; }
        [NSerialize(1)] public BoneType ParentBone { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1819:PropertiesShouldNotReturnArrays", Justification = "<Pending>")]
        [NSerialize(2)]
        public float[] LocalTransform { get; set; } = new float[16];
    }
}