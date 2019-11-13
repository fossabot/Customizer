using System;
using Customizer.Utility;

namespace Customizer.Modeling {
    /// <summary>
    /// Stores mesh data
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields",
        Justification = "<Pending>")]
    [CzCustomSerializeMembers]
    public sealed class LiveMesh : ICloneable {
        /// <summary>
        /// Bones in this mesh
        /// </summary>
        [CzSerialize(0)] public Bone[] Bones;

        /// <summary>
        /// Sub-meshes in this mesh
        /// </summary>
        [CzSerialize(1)] public LiveSubMesh[] SubMeshes;

        /// <summary>
        /// Attach points in this mesh
        /// </summary>
        [CzSerialize(2)] public AttachPoint[] DefaultAttachPoints;

        /// <summary>
        /// This mesh's unique name
        /// </summary>
        [CzSerialize(3)] public string UniqueName;

        /// <summary>
        /// The base variant type name
        /// </summary>
        [CzSerialize(4)] public string VariantTypeName;

        /// <summary>
        /// The unique name of the mesh originally fit against
        /// </summary>
        [CzSerialize(5)] public string FitUniqueName;

        public object Clone() {
            var res = new LiveMesh {
                Bones = new Bone[Bones.Length],
                SubMeshes = new LiveSubMesh[SubMeshes.Length],
                DefaultAttachPoints = new AttachPoint[DefaultAttachPoints.Length],
                UniqueName = UniqueName,
                VariantTypeName = VariantTypeName,
                FitUniqueName = FitUniqueName
            };
            Array.Copy(Bones, res.Bones, Bones.Length);
            for (var i = 0; i < DefaultAttachPoints.Length; i++)
                res.SubMeshes[i] = (LiveSubMesh) SubMeshes[i].Clone();
            for (var i = 0; i < DefaultAttachPoints.Length; i++)
                res.DefaultAttachPoints[i] = (AttachPoint) DefaultAttachPoints[i].Clone();
            return res;
        }
    }
}