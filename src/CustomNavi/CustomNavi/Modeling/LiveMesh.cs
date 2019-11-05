using System.Numerics;
using CustomNavi.Utility;

namespace CustomNavi.Modeling {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields",
        Justification = "<Pending>")]
    [NCustomSerializeMembers]
    public class LiveMesh {
        [NSerialize(0)] public Matrix4x4[] BindPoses;
        [NSerialize(1)] public AttachPoint[] DefaultAttachPoints;
        [NSerialize(2)] public Bone[] Bones;
        [NSerialize(3)] public LiveSubMesh[] SubMeshes;
    }
}