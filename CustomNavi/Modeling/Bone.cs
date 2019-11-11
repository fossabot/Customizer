using System.Numerics;
using CustomNavi.Utility;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace CustomNavi.Modeling {
    /// <summary>
    /// Stores information about bone on a mesh
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance",
        "CA1815:Override equals and operator equals on value types", Justification = "<Pending>")]
    [CnCustomSerializeMembers]
    public struct Bone {
        /// <summary>
        /// Type of bone this instance represents
        /// </summary>
        [CnSerialize(0)]
        public BoneType Type { get; set; }

        /// <summary>
        /// Name of original bone on mesh
        /// </summary>
        [CnSerialize(1)]
        public string BoneName { get; set; }

        /// <summary>
        /// 4x4 float matrix with base transform
        /// </summary>
        [CnSerialize(2)]
        public Matrix4x4 BindPose { get; set; }
    }
}