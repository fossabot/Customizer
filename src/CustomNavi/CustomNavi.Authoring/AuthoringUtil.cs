using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text.RegularExpressions;
using Assimp;
using CustomNavi.Modeling;
using Bone = Assimp.Bone;
using Matrix4x4 = System.Numerics.Matrix4x4;

namespace CustomNavi.Authoring {
    public static class AuthoringUtil {
        public static LiveMesh GenerateLiveMesh(Stream stream, List<Tuple<string, BoneType>> boneRegexs = null,
            List<Tuple<string, AttachPointType>> attachPointRegexs = null) {
            using var ctx = new AssimpContext();
            var scene = ctx.ImportFileFromStream(stream, PostProcessSteps.Triangulate, "fbx");
            var bones = new List<Bone>();
            var mBones = new List<Modeling.Bone>();
            var attachPoints = new List<AttachPoint>();
            var subMeshes = new LiveSubMesh[scene.MeshCount];
            for (var iSubMesh = 0; iSubMesh < subMeshes.Length; iSubMesh++) {
                // Define submesh
                var subMesh = scene.Meshes[iSubMesh];
                var vertices = new Vector3[subMesh.VertexCount];
                var uvs = new Vector2[subMesh.VertexCount];
                var normals = new Vector3[subMesh.VertexCount];
                var weights = new BoneWeight[subMesh.VertexCount];
                var triangles = new int[subMesh.FaceCount * 3];
                var mSubMesh = subMeshes[iSubMesh] = new LiveSubMesh
                    {Vertices = vertices, UVs = uvs, Normals = normals, BoneWeights = weights, Triangles = triangles};
                // Get vertices
                for (var iVertex = 0; iVertex < vertices.Length; iVertex++)
                    vertices[iVertex] = subMesh.Vertices[iVertex].ToVector3();
                // Get UVs
                for (var iUv = 0; iUv < uvs.Length; iUv++)
                    uvs[iUv] = subMesh.TextureCoordinateChannels[0][iUv].ToVector2();
                // Get normals
                for (var iNormal = 0; iNormal < normals.Length; iNormal++)
                    normals[iNormal] = subMesh.Normals[iNormal].ToVector3();
                // Get triangles
                for (var iTriangle = 0; iTriangle < subMesh.FaceCount; iTriangle++) {
                    var x = subMesh.Faces[iTriangle];
                    for (var i = 0; i < 3; i++)
                        triangles[3 * iTriangle + i] = x.Indices[i];
                }

                // Get material index
                mSubMesh.MaterialIdx = subMesh.MaterialIndex;
                // Get bones
                foreach (var bone in subMesh.Bones) {
                    foreach (var vWeight in bone.VertexWeights) {
                        var weight = weights[vWeight.VertexID];
                        var bId = bones.IndexOf(bone);
                        var bWe = vWeight.Weight;
                        switch (weight.count) {
                            case 0:
                                weight.bone1 = bId;
                                weight.weight1 = bWe;
                                break;
                            case 1:
                                weight.bone2 = bId;
                                weight.weight2 = bWe;
                                break;
                            case 2:
                                weight.bone3 = bId;
                                weight.weight3 = bWe;
                                break;
                            case 3:
                                weight.bone4 = bId;
                                weight.weight4 = bWe;
                                break;
                        }

                        if (weight.count != 4)
                            weight.count++;
                    }

                    // Skip bone if already processed
                    if (bones.Contains(bone)) continue;
                    // Add bone
                    var mBone = new Modeling.Bone {BoneName = bone.Name, BindPose = bone.OffsetMatrix.ToMatrix4x4()};
                    if (boneRegexs != null)
                        foreach (var (item1, item2) in boneRegexs)
                            if (Regex.IsMatch(bone.Name, item1)) {
                                mBone.Type = item2;
                                break;
                            }

                    bones.Add(bone);
                    mBones.Add(mBone);
                    // Add attach point if applicable
                    if (attachPointRegexs == null) continue;
                    foreach (var (item1, item2) in attachPointRegexs)
                        if (Regex.IsMatch(bone.Name, item1)) {
                            attachPoints.Add(new AttachPoint {
                                BoneName = bone.Name, BindPose = bone.OffsetMatrix.ToMatrix4x4Array(), Type = item2
                            });
                            break;
                        }
                }
            }

            return new LiveMesh {
                Bones = mBones.ToArray(), DefaultAttachPoints = attachPoints.ToArray(), SubMeshes = subMeshes
            };
        }

        private static Vector2 ToVector2(this Vector3D vector)
            => new Vector2(vector.X, vector.Y);

        private static Vector3 ToVector3(this Vector3D vector)
            => new Vector3(vector.X, vector.Y, vector.Z);

        // ReSharper disable InconsistentNaming
        private static Matrix4x4 ToMatrix4x4(this Assimp.Matrix4x4 matrix) =>
            new Matrix4x4(matrix[1, 1], matrix[1, 2], matrix[1, 3], matrix[1, 4], matrix[2, 1], matrix[2, 2],
                matrix[2, 3], matrix[2, 4], matrix[3, 1], matrix[3, 2], matrix[3, 3], matrix[3, 4], matrix[4, 1],
                matrix[4, 2], matrix[4, 3], matrix[4, 4]);

        private static float[] ToMatrix4x4Array(this Assimp.Matrix4x4 matrix) =>
            new[] {
                matrix[1, 1], matrix[1, 2], matrix[1, 3], matrix[1, 4], matrix[2, 1], matrix[2, 2],
                matrix[2, 3], matrix[2, 4], matrix[3, 1], matrix[3, 2], matrix[3, 3], matrix[3, 4], matrix[4, 1],
                matrix[4, 2], matrix[4, 3], matrix[4, 4]
            };
        // ReSharper restore InconsistentNaming
    }
}