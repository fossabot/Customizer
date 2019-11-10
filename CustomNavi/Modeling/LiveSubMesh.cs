using System.Numerics;
using CustomNavi.Utility;

namespace CustomNavi.Modeling {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields",
        Justification = "<Pending>")]
    [CnCustomSerializeMembers]
    public class LiveSubMesh {
        [CnSerialize(0)] public Vector3[] Vertices;
        [CnSerialize(1)] public Vector2[] UVs;
        [CnSerialize(2)] public Vector3[] Normals;
        [CnSerialize(3)] public BoneWeight[] BoneWeights;
        [CnSerialize(4)] public int[] Triangles;
        [CnSerialize(5)] public int MaterialIdx;
    }
}