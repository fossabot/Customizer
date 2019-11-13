using System;
using System.Collections.Generic;
using System.IO;
//using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Assimp;
using Customizer.Modeling;
using Bone = Assimp.Bone;
using Matrix4x4 = System.Numerics.Matrix4x4;

namespace Customizer.Authoring {
    /// <summary>
    /// Utility class for content authoring
    /// </summary>
    public static class AuthoringUtil {
        /// <summary>
        /// Generate LiveMesh instance from FBX data stream
        /// </summary>
        /// <param name="stream">FBX data to read</param>
        /// <param name="boneRegexs">Optional bone regex mappings</param>
        /// <param name="attachPointRegexs">Optional attach point regex mappings</param>
        /// <returns>LiveMesh instance</returns>
        public static LiveMesh GenerateLiveMesh(Stream stream, Dictionary<string, BoneType> boneRegexs = null,
            Dictionary<string, AttachPointType> attachPointRegexs = null) {
            Scene scene;
            // API goes into native code to load FBX
            using (var ctx = new AssimpContext())
                scene = ctx.ImportFileFromStream(stream, PostProcessSteps.Triangulate, "fbx");
            var bones = new List<Bone>();
            var mBones = new List<Modeling.Bone>();
            var attachPoints = new List<AttachPoint>();
            var subMeshes = new LiveSubMesh[scene.MeshCount];
            // Load sub-meshes
            for (var iSubMesh = 0; iSubMesh < subMeshes.Length; iSubMesh++) {
                // Define sub-mesh
                var subMesh = scene.Meshes[iSubMesh];
                //var vertices = new Vector3[subMesh.VertexCount];
                var vertices = new float[subMesh.VertexCount * 3];
                //var uvs = new Vector2[subMesh.VertexCount];
                var uvs = new float[subMesh.VertexCount * 2];
                //var normals = new Vector3[subMesh.VertexCount];
                var normals = new float[subMesh.VertexCount * 3];
                var weights = new BoneWeight[subMesh.VertexCount];
                var triangles = new int[subMesh.FaceCount * 3];
                subMeshes[iSubMesh] = new LiveSubMesh {
                    Vertices = vertices, UVs = uvs, Normals = normals, BoneWeights = weights, Triangles = triangles,
                    MaterialIdx = subMesh.MaterialIndex, VertexCount = subMesh.VertexCount
                };
                // Get vertices
                //for (var iVertex = 0; iVertex < vertices.Length; iVertex++)
                //    vertices[iVertex] = subMesh.Vertices[iVertex].ToVector3();
                for (var iVertex = 0; iVertex < subMesh.Vertices.Count; iVertex++) {
                    vertices[iVertex * 3] = subMesh.Vertices[iVertex].X;
                    vertices[iVertex * 3 + 1] = subMesh.Vertices[iVertex].Y;
                    vertices[iVertex * 3 + 2] = subMesh.Vertices[iVertex].Z;
                }

                // Get UVs
                //for (var iUv = 0; iUv < uvs.Length; iUv++)
                //    uvs[iUv] = subMesh.TextureCoordinateChannels[0][iUv].ToVector2();
                for (var iUv = 0; iUv < subMesh.TextureCoordinateChannels[0].Count; iUv++) {
                    uvs[iUv * 2] = subMesh.TextureCoordinateChannels[0][iUv].X;
                    uvs[iUv * 2 + 1] = subMesh.TextureCoordinateChannels[0][iUv].X;
                }

                // Get normals
                //for (var iNormal = 0; iNormal < normals.Length; iNormal++)
                //    normals[iNormal] = subMesh.Normals[iNormal].ToVector3();
                for (var iNormal = 0; iNormal < subMesh.Normals.Count; iNormal++) {
                    normals[iNormal * 3] = subMesh.Normals[iNormal].X;
                    normals[iNormal * 3 + 1] = subMesh.Normals[iNormal].Y;
                    normals[iNormal * 3 + 2] = subMesh.Normals[iNormal].Z;
                }

                // Get triangles
                for (var iTriangle = 0; iTriangle < subMesh.FaceCount; iTriangle++) {
                    var x = subMesh.Faces[iTriangle];
                    for (var i = 0; i < 3; i++)
                        triangles[3 * iTriangle + i] = x.Indices[i];
                }

                // Get bones
                foreach (var bone in subMesh.Bones) {
                    foreach (var vWeight in bone.VertexWeights) {
                        var weight = weights[vWeight.VertexID];
                        var bId = bones.IndexOf(bone);
                        var bWe = vWeight.Weight;
                        // ReSharper disable once SwitchStatementMissingSomeCases
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
                        weights[vWeight.VertexID] = weight;
                    }

                    // Skip bone if already processed
                    if (bones.Contains(bone)) continue;
                    // Add bone
                    var mBone = new Modeling.Bone {BoneName = bone.Name, BindPose = bone.OffsetMatrix.ToMatrix4x4()};
                    if (boneRegexs != null)
                        foreach (var entry in boneRegexs)
                            if (Regex.IsMatch(bone.Name, entry.Key)) {
                                mBone.Type = entry.Value;
                                break;
                            }

                    bones.Add(bone);
                    mBones.Add(mBone);
                    // Add attach point if applicable
                    if (attachPointRegexs == null) continue;
                    foreach (var entry in attachPointRegexs)
                        if (Regex.IsMatch(bone.Name, entry.Key)) {
                            attachPoints.Add(new AttachPoint {
                                BoneName = bone.Name,
                                BindPose = bone.OffsetMatrix.ToMatrix4x4Array(),
                                Type = entry.Value
                            });
                            break;
                        }
                }
            }

            return new LiveMesh {
                Bones = mBones.ToArray(),
                DefaultAttachPoints = attachPoints.ToArray(),
                SubMeshes = subMeshes
            };
        }

        /// <summary>
        /// Deserialize object from JSON stream
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <returns>Deserialized object</returns>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is null</exception>
        public static T JsonDeserialize<T>(Stream stream) {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            using (var ms = new MemoryStream()) {
                stream.CopyTo(ms);
                var opts = new JsonSerializerOptions();
                opts.Converters.Add(new JsonStringEnumConverter());
                return JsonSerializer.Deserialize<T>(ms.ToArray(), opts);
            }
        }

        /// <summary>
        /// Serialize object to JSON stream
        /// </summary>
        /// <param name="value">Definition to serialize</param>
        /// <param name="stream">Stream to write to</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> or <paramref name="stream"/> are null</exception>
        public static void JsonSerialize(object value, Stream stream) {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            var opts = new JsonSerializerOptions {WriteIndented = true};
            opts.Converters.Add(new JsonStringEnumConverter());
            using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions {Indented = true})) {
                JsonSerializer.Serialize(writer, value, opts);
            }
        }

        /*
        private static Vector2 ToVector2(this Vector3D vector)
            => new Vector2(vector.X, vector.Y);

        private static Vector3 ToVector3(this Vector3D vector)
            => new Vector3(vector.X, vector.Y, vector.Z);
        */

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