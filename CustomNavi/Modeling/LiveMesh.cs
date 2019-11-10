using CustomNavi.Utility;

namespace CustomNavi.Modeling {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields",
        Justification = "<Pending>")]
    [CnCustomSerializeMembers]
    public class LiveMesh {
        [CnSerialize(0)] public Bone[] Bones;
        [CnSerialize(1)] public LiveSubMesh[] SubMeshes;
        [CnSerialize(2)] public AttachPoint[] DefaultAttachPoints;
    }
}