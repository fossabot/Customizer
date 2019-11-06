using System.Numerics;
using CustomNavi.Utility;

namespace CustomNavi.Modeling {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields",
        Justification = "<Pending>")]
    [NCustomSerializeMembers]
    public class LiveSubMesh {
        [NSerialize(0)] public Vector3[] Vertices;
        [NSerialize(1)] public Vector2[] UVs;
        [NSerialize(2)] public Vector3[] Normals;
        [NSerialize(3)] public BoneWeight[] BoneWeights;
        [NSerialize(4)] public int[] Triangles;
        [NSerialize(5)] public int MaterialIdx;
    }
}