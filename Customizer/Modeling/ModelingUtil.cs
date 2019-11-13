using System;

namespace Customizer.Modeling {
    public static class ModelUtil {
        /// <summary>
        /// Refit mesh against mesh variant originally used 
        /// </summary>
        /// <param name="baseVariant">Original variant</param>
        /// <param name="targetVariant">Target variant</param>
        /// <param name="sourceMesh">Mesh to modify</param>
        /// <param name="options">Processing configuration</param>
        /// <returns></returns>
        public static LiveMesh Refit(LiveMesh baseVariant, LiveMesh targetVariant, LiveMesh sourceMesh,
            RefitOptions options) {
            if (baseVariant == null)
                throw new ArgumentNullException(nameof(baseVariant));
            if (targetVariant == null)
                throw new ArgumentNullException(nameof(targetVariant));
            if (sourceMesh == null)
                throw new ArgumentNullException(nameof(sourceMesh));
            var res = (LiveMesh) sourceMesh.Clone();
            res.FitUniqueName = baseVariant.UniqueName;
            Span<float> delta = stackalloc float[3];
            foreach (var sub in res.SubMeshes) {
                for (var iSubMesh = 0; iSubMesh < sub.VertexCount; iSubMesh++) {
                    var closestBaseIdx = 0;
                    var closestBaseVert = 0;
                    var closestLenSq = float.MaxValue;
                    for (var iBaseIdx = 0; iBaseIdx < baseVariant.SubMeshes.Length; iBaseIdx++) {
                        var baseSubMesh = baseVariant.SubMeshes[iBaseIdx];
                        for (var iBaseVert = 0; iBaseVert < baseSubMesh.VertexCount; iBaseVert++) {
                            var curLenSq = SqDist(sub.Vertices, iSubMesh, baseSubMesh.Vertices, iBaseVert, 3);
                            if (!(curLenSq < closestLenSq)) continue;
                            closestBaseIdx = iBaseIdx;
                            closestBaseVert = iBaseVert;
                            closestLenSq = curLenSq;
                        }
                    }

                    // Placeholder basic single-vertex delta
                    Sub(baseVariant.SubMeshes[closestBaseIdx].Vertices, closestBaseVert,
                        targetVariant.SubMeshes[closestBaseIdx].Vertices, closestBaseVert, delta, 0, 3);
                    Add(sub.Vertices, iSubMesh, delta, 0, sub.Vertices, iSubMesh, 3);
                    // TODO "better" face-based transform
                    // Determine face
                    // Procedural transformation
                }
            }

            return res;
        }

        private static float SqDist(Span<float> lhs, int lhsOffs, Span<float> rhs, int rhsOffs, int stride) {
            var res = 0.0f;
            for (var i = 0; i < stride; i++) {
                var tmp = lhs[stride * lhsOffs + i] - rhs[stride * rhsOffs + i];
                res += tmp * tmp;
            }

            return res;
        }

        private static void Add(Span<float> lhs, int lhsOffs, Span<float> rhs, int rhsOffs, Span<float> res,
            int resOffs,
            int stride) {
            for (var i = 0; i < stride; i++)
                res[stride * resOffs + i] = lhs[stride * lhsOffs + i] + rhs[stride * rhsOffs + i];
        }

        private static void Sub(Span<float> lhs, int lhsOffs, Span<float> rhs, int rhsOffs, Span<float> res,
            int resOffs,
            int stride) {
            for (var i = 0; i < stride; i++)
                res[stride * resOffs + i] = lhs[stride * lhsOffs + i] - rhs[stride * rhsOffs + i];
        }
    }
}