using CustomNavi.Utility;

namespace CustomNavi.Modeling {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance",
        "CA1815:Override equals and operator equals on value types", Justification = "<Pending>")]
    [NCustomSerializeMembers]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class AttachPoint {
        [NSerialize(0)] public AttachPointType Type { get; set; }
        [NSerialize(1)] public string BoneName { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1819:PropertiesShouldNotReturnArrays", Justification = "<Pending>")]
        [NSerialize(2)]
        public float[] BindPose { get; set; } = new float[16];
    }
}