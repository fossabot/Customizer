using CustomNavi.Utility;

namespace CustomNavi.Modeling {
    /// <summary>
    /// Stores mesh data
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields",
        Justification = "<Pending>")]
    [CnCustomSerializeMembers]
    public class LiveMesh {
        /// <summary>
        /// Bones in this mesh
        /// </summary>
        [CnSerialize(0)] public Bone[] Bones;

        /// <summary>
        /// Sub-meshes in this mesh
        /// </summary>
        [CnSerialize(1)] public LiveSubMesh[] SubMeshes;

        /// <summary>
        /// Attach points in this mesh
        /// </summary>
        [CnSerialize(2)] public AttachPoint[] DefaultAttachPoints;
    }
}