using System.Numerics;
using CustomNavi.Utility;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace CustomNavi.Modeling {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance",
        "CA1815:Override equals and operator equals on value types", Justification = "<Pending>")]
    [CnCustomSerializeMembers]
    public struct Bone {
        [CnSerialize(0)] public BoneType Type { get; set; }
        [CnSerialize(1)] public string BoneName { get; set; }
        [CnSerialize(2)] public Matrix4x4 BindPose { get; set; }
    }
}