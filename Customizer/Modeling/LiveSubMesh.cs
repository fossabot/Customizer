using System.Numerics;
using Customizer.Utility;

namespace Customizer.Modeling {
    /// <summary>
    /// Stores raw mesh data for a sub-mesh
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields",
        Justification = "<Pending>")]
    [CzCustomSerializeMembers]
    public class LiveSubMesh {
        /// <summary>
        /// Vertices in sub-mesh
        /// </summary>
        [CzSerialize(0)] public Vector3[] Vertices;

        /// <summary>
        /// Vertex-indexed UVs in sub-mesh
        /// </summary>
        [CzSerialize(1)] public Vector2[] UVs;

        /// <summary>
        /// Vertex-indexed normals in sub-mesh
        /// </summary>
        [CzSerialize(2)] public Vector3[] Normals;

        /// <summary>
        /// Vertex-indexed bone weights in sub-mesh
        /// </summary>
        [CzSerialize(3)] public BoneWeight[] BoneWeights;

        /// <summary>
        /// Triangles in sub-mesh
        /// </summary>
        [CzSerialize(4)] public int[] Triangles;

        /// <summary>
        /// Material index for sub-mesh
        /// </summary>
        [CzSerialize(5)] public int MaterialIdx;
    }
}