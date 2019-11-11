using System.Numerics;
using Customizer.Utility;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Customizer.Modeling {
    /// <summary>
    /// Stores information about bone on a mesh
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance",
        "CA1815:Override equals and operator equals on value types", Justification = "<Pending>")]
    [CzCustomSerializeMembers]
    public struct Bone {
        /// <summary>
        /// Type of bone this instance represents
        /// </summary>
        [CzSerialize(0)]
        public BoneType Type { get; set; }

        /// <summary>
        /// Name of original bone on mesh
        /// </summary>
        [CzSerialize(1)]
        public string BoneName { get; set; }

        /// <summary>
        /// 4x4 float matrix with base transform
        /// </summary>
        [CzSerialize(2)]
        public Matrix4x4 BindPose { get; set; }
    }
}