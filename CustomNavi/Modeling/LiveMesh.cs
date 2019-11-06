using CustomNavi.Utility;

namespace CustomNavi.Modeling {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields",
        Justification = "<Pending>")]
    [NCustomSerializeMembers]
    public class LiveMesh {
        [NSerialize(0)] public Bone[] Bones;
        [NSerialize(1)] public LiveSubMesh[] SubMeshes;
        [NSerialize(2)] public AttachPoint[] DefaultAttachPoints;
    }
}