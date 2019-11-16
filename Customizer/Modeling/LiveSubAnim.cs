using Customizer.Utility;

namespace Customizer.Modeling {
    /// <summary>
    /// Stores animation data for one bone
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields",
        Justification = "<Pending>")]
    [CzCustomSerializeMembers]
    public class LiveSubAnim {
        [CzSerialize(0)] public int BoneId;
        [CzSerialize(1)] public int PositionCount;
        [CzSerialize(2)] public float[] PositionTimes;
        [CzSerialize(3)] public float[] PositionX;
        [CzSerialize(4)] public float[] PositionY;
        [CzSerialize(5)] public float[] PositionZ;
        [CzSerialize(6)] public int RotationCount;
        [CzSerialize(7)] public float[] RotationTimes;
        [CzSerialize(8)] public float[] RotationW;
        [CzSerialize(9)] public float[] RotationX;
        [CzSerialize(10)] public float[] RotationY;
        [CzSerialize(11)] public float[] RotationZ;
        [CzSerialize(12)] public int ScalingCount;
        [CzSerialize(13)] public float[] ScalingTimes;
        [CzSerialize(14)] public float[] ScalingX;
        [CzSerialize(15)] public float[] ScalingY;
        [CzSerialize(16)] public float[] ScalingZ;
    }
}