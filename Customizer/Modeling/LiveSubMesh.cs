using System;
using Customizer.Utility;

namespace Customizer.Modeling {
    /// <summary>
    /// Stores raw mesh data for a sub-mesh
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields",
        Justification = "<Pending>")]
    [CzCustomSerializeMembers]
    public sealed class LiveSubMesh : ICloneable {
        /// <summary>
        /// Vertices in sub-mesh
        /// </summary>
        //[CzSerialize(0)] public Vector3[] Vertices;
        [CzSerialize(0)] public float[] Vertices;

        /// <summary>
        /// Vertex-indexed UVs in sub-mesh
        /// </summary>
        //[CzSerialize(1)] public Vector2[] UVs;
        [CzSerialize(1)] public float[] UVs;

        /// <summary>
        /// Vertex-indexed normals in sub-mesh
        /// </summary>
        //[CzSerialize(2)] public Vector3[] Normals;
        [CzSerialize(2)] public float[] Normals;

        /// <summary>
        /// Vertex-indexed bone IDs in sub-mesh
        /// </summary>
        [CzSerialize(3)] public int[] BoneIds;
        
        
        /// <summary>
        /// Vertex-indexed bone weights in sub-mesh
        /// </summary>
        [CzSerialize(4)] public float[] BoneWeights;

        /// <summary>
        /// Triangles in sub-mesh
        /// </summary>
        [CzSerialize(5)] public int[] Triangles;

        /// <summary>
        /// Material index for sub-mesh
        /// </summary>
        [CzSerialize(6)] public int MaterialIdx;
        
        /// <summary>
        /// Number of vertices in sub-mesh
        /// </summary>
        [CzSerialize(7)] public int VertexCount;

        public object Clone() {
            var res = new LiveSubMesh {
                Vertices = new float[Vertices.Length],
                UVs = new float[UVs.Length],
                Normals = new float[Normals.Length],
                BoneIds = new int[BoneIds.Length],
                BoneWeights = new float[BoneWeights.Length],
                Triangles = new int[Triangles.Length],
                MaterialIdx = MaterialIdx,
                VertexCount = VertexCount
            };
            Array.Copy(Vertices, res.Vertices, Vertices.Length);
            Array.Copy(UVs, res.UVs, UVs.Length);
            Array.Copy(Normals, res.Normals, Normals.Length);
            Array.Copy(BoneIds, res.BoneIds, BoneIds.Length);
            Array.Copy(BoneWeights, res.BoneWeights, BoneWeights.Length);
            Array.Copy(Triangles, res.Triangles, Triangles.Length);
            return res;
        }
    }
}