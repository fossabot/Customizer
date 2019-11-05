using CustomNavi.Utility;

namespace CustomNavi.Modeling {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance",
        "CA1815:Override equals and operator equals on value types", Justification = "<Pending>")]
    [NCustomSerializeMembers]
    public struct Bone {
        [NSerialize(0)] public BoneType Type { get; set; }
        [NSerialize(1)] public string BoneName { get; set; }
        [NSerialize(2)] public int BindPoseIdx { get; set; }
        [NSerialize(3)] public int ParentBoneIdx { get; set; }
    }
}