using CustomNavi.Utility;

namespace CustomNavi.Modeling {
    /// <summary>
    /// Stores information about attach point on a mesh
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance",
        "CA1815:Override equals and operator equals on value types", Justification = "<Pending>")]
    [CnCustomSerializeMembers]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class AttachPoint {
        /// <summary>
        /// Type of attach point this instance represents
        /// </summary>
        [CnSerialize(0)]
        public AttachPointType Type { get; set; }

        /// <summary>
        /// Name of original bone on mesh (or user-defined)
        /// </summary>
        [CnSerialize(1)]
        public string BoneName { get; set; }

        /// <summary>
        /// 4x4 float matrix with base transform
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1819:PropertiesShouldNotReturnArrays", Justification = "<Pending>")]
        [CnSerialize(2)]
        public float[] BindPose { get; set; } = new float[16];
    }
}