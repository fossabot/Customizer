using System;
using System.Collections.Generic;
using System.IO;
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
        /// <param name="debugLog">Debug log output function</param>
        /// <returns>LiveMesh instance</returns>
        public static LiveMesh GenerateLiveMesh(Stream stream, Dictionary<string, BoneType> boneRegexs = null,
            Dictionary<string, AttachPointType> attachPointRegexs = null, Action<string> debugLog = null) {
            // Relevant assimp documentation can be found here:
            // http://assimp.sourceforge.net/lib_html/data.html
            Scene scene;
            // API goes into native code to load FBX
            using (var ctx = new AssimpContext())
                scene = ctx.ImportFileFromStream(stream, PostProcessSteps.Triangulate, "fbx");
            var bones = new Dictionary<Bone, int>();
            var mBones = new List<Modeling.Bone>();
            var uCount = 0;
            var uACount = 0;
            var attachPoints = new List<AttachPoint>();
            var subMeshes = new LiveSubMesh[scene.MeshCount];
            debugLog?.Invoke($"Sub-meshes: {scene.MeshCount}");
            // Load sub-meshes
            for (var iSubMesh = 0; iSubMesh < subMeshes.Length; iSubMesh++) {
                // Define sub-mesh
                var subMesh = scene.Meshes[iSubMesh];
                debugLog?.Invoke($"-Sub-mesh [{subMesh.Name}]");
                var vertices = new float[subMesh.VertexCount * 3];
                var uvs = new float[subMesh.VertexCount * 2];
                var normals = new float[subMesh.VertexCount * 3];
                var boneIds = new int[subMesh.VertexCount * 4];
                var boneWeights = new float[subMesh.VertexCount * 4];
                var boneCounts = new int[subMesh.VertexCount];
                var triangles = new int[subMesh.FaceCount * 3];
                subMeshes[iSubMesh] = new LiveSubMesh {
                    Vertices = vertices, UVs = uvs, Normals = normals, BoneIds = boneIds, BoneWeights = boneWeights,
                    Triangles = triangles,
                    MaterialIdx = subMesh.MaterialIndex, VertexCount = subMesh.VertexCount
                };
                // Get vertices
                debugLog?.Invoke($"--Vertices: {subMesh.VertexCount}");
                for (var iVertex = 0; iVertex < subMesh.Vertices.Count; iVertex++) {
                    vertices[iVertex * 3] = subMesh.Vertices[iVertex].X;
                    vertices[iVertex * 3 + 1] = subMesh.Vertices[iVertex].Y;
                    vertices[iVertex * 3 + 2] = subMesh.Vertices[iVertex].Z;
                }

                // Get UVs
                for (var iUv = 0; iUv < subMesh.TextureCoordinateChannels[0].Count; iUv++) {
                    uvs[iUv * 2] = subMesh.TextureCoordinateChannels[0][iUv].X;
                    uvs[iUv * 2 + 1] = subMesh.TextureCoordinateChannels[0][iUv].X;
                }

                // Get normals
                for (var iNormal = 0; iNormal < subMesh.Normals.Count; iNormal++) {
                    normals[iNormal * 3] = subMesh.Normals[iNormal].X;
                    normals[iNormal * 3 + 1] = subMesh.Normals[iNormal].Y;
                    normals[iNormal * 3 + 2] = subMesh.Normals[iNormal].Z;
                }

                // Get triangles
                debugLog?.Invoke($"--Triangles: {subMesh.FaceCount}");
                for (var iTriangle = 0; iTriangle < subMesh.FaceCount; iTriangle++) {
                    var x = subMesh.Faces[iTriangle];
                    for (var i = 0; i < 3; i++)
                        triangles[3 * iTriangle + i] = x.Indices[i];
                }

                // Get bones
                debugLog?.Invoke($"--Bones: {subMesh.BoneCount}");
                foreach (var bone in subMesh.Bones) {
                    foreach (var vWeight in bone.VertexWeights) {
                        var count = boneCounts[vWeight.VertexID];
                        if (count == 4) continue;
                        boneIds[4 * vWeight.VertexID + count] =
                            bones.TryGetValue(bone, out var bId) ? bId : bones.Count;
                        boneWeights[4 * vWeight.VertexID + count] = vWeight.Weight;
                        boneCounts[vWeight.VertexID]++;
                    }

                    // Skip bone if already processed
                    if (bones.ContainsKey(bone)) continue;
                    debugLog?.Invoke($"{{Bone [{bone.Name}]}}");
                    // Add bone
                    var mBone = new Modeling.Bone {BoneName = bone.Name, BindPose = bone.OffsetMatrix.ToMatrix4x4()};
                    if (boneRegexs != null)
                        foreach (var entry in boneRegexs)
                            if (Regex.IsMatch(bone.Name, entry.Key)) {
                                mBone.Type = entry.Value;
                                uCount++;
                                break;
                            }

                    bones.Add(bone, bones.Count);
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
                            uACount++;
                            break;
                        }
                }
            }

            debugLog?.Invoke($"Total stored bones: {mBones.Count}");
            debugLog?.Invoke($"Total BoneType-matched bones: {uCount}");
            debugLog?.Invoke($"Total AttachPointType-matched bones: {uACount}");

            return new LiveMesh {
                Bones = mBones.ToArray(),
                DefaultAttachPoints = attachPoints.ToArray(),
                SubMeshes = subMeshes
            };
        }

        /// <summary>
        /// Generate LiveAnim instance from FBX data stream
        /// </summary>
        /// <param name="stream">FBX data to read</param>
        /// <param name="boneRegexs">Optional bone regex mappings</param>
        /// <param name="debugLog">Debug log output function</param>
        /// <returns>LiveAnim instance</returns>
        public static LiveAnim GenerateLiveAnim(Stream stream, Dictionary<string, BoneType> boneRegexs = null,
            Action<string> debugLog = null) {
            // Relevant assimp documentation can be found here:
            // http://assimp.sourceforge.net/lib_html/data.html
            Scene scene;
            // API goes into native code to load FBX
            using (var ctx = new AssimpContext())
                scene = ctx.ImportFileFromStream(stream, PostProcessSteps.Triangulate, "fbx");
            var bones = new Dictionary<string, int>();
            var mBones = new List<Modeling.Bone>();
            var uCount = 0;
            var subAnims = new List<LiveSubAnim>();
            debugLog?.Invoke($"Sub-meshes: {scene.MeshCount}");
            // Get bones
            foreach (var subMesh in scene.Meshes) {
                debugLog?.Invoke($"-Sub-mesh [{subMesh.Name}]");
                debugLog?.Invoke($"--Bones: {subMesh.BoneCount}");
                foreach (var bone in subMesh.Bones) {
                    // Skip bone if already processed
                    if (bones.ContainsKey(bone.Name)) continue;
                    debugLog?.Invoke($"{{Bone [{bone.Name}]}}");
                    // Add bone
                    var mBone = new Modeling.Bone {BoneName = bone.Name, BindPose = bone.OffsetMatrix.ToMatrix4x4()};
                    if (boneRegexs != null)
                        foreach (var entry in boneRegexs)
                            if (Regex.IsMatch(bone.Name, entry.Key)) {
                                mBone.Type = entry.Value;
                                uCount++;
                                break;
                            }

                    bones.Add(bone.Name, bones.Count);
                    mBones.Add(mBone);
                }
            }

            debugLog?.Invoke($"Total stored bones: {mBones.Count}");
            debugLog?.Invoke($"Total BoneType-matched bones: {uCount}");

            // ?? Potential: Add internal structure for full mesh hierarchy with bone toggles (sort of like suggested)
            foreach (var anim in scene.Animations) {
                debugLog?.Invoke($"-Animation [{anim.Name}]");
                foreach (var chan in anim.NodeAnimationChannels) {
                    debugLog?.Invoke($"--Channel [{chan.NodeName}]");
                    // Hacky name matching
                    var nodeName = chan.NodeName;
                    var idxAssimp = nodeName.IndexOf("_$AssimpFbx$_", StringComparison.Ordinal);
                    if (idxAssimp != -1)
                        nodeName = nodeName.Substring(0, idxAssimp);
                    var sub = new LiveSubAnim {BoneId = bones[nodeName]};
                    if (chan.HasPositionKeys) {
                        debugLog?.Invoke($"---Position keys: {chan.PositionKeyCount}");
                        sub.PositionCount = chan.PositionKeyCount;
                        sub.PositionTimes = new float[chan.PositionKeyCount];
                        sub.PositionX = new float[chan.PositionKeyCount];
                        sub.PositionY = new float[chan.PositionKeyCount];
                        sub.PositionZ = new float[chan.PositionKeyCount];
                        for (var i = 0; i < chan.PositionKeyCount; i++) {
                            sub.PositionTimes[i] = (float) chan.PositionKeys[i].Time;
                            var vec = chan.PositionKeys[i].Value;
                            sub.PositionX[i] = vec.X;
                            sub.PositionY[i] = vec.Y;
                            sub.PositionZ[i] = vec.Z;
                        }
                    }

                    if (chan.HasRotationKeys) {
                        debugLog?.Invoke($"---Rotation keys: {chan.RotationKeyCount}");
                        sub.RotationCount = chan.RotationKeyCount;
                        sub.RotationTimes = new float[chan.RotationKeyCount];
                        sub.RotationW = new float[chan.PositionKeyCount];
                        sub.RotationX = new float[chan.PositionKeyCount];
                        sub.RotationY = new float[chan.PositionKeyCount];
                        sub.RotationZ = new float[chan.PositionKeyCount];
                        for (var i = 0; i < chan.RotationKeyCount; i++) {
                            sub.RotationTimes[i] = (float) chan.RotationKeys[i].Time;
                            var qua = chan.RotationKeys[i].Value;
                            sub.RotationW[i] = qua.W;
                            sub.RotationX[i] = qua.X;
                            sub.RotationY[i] = qua.Y;
                            sub.RotationZ[i] = qua.Z;
                        }
                    }

                    if (chan.HasScalingKeys) {
                        debugLog?.Invoke($"---Scaling keys: {chan.ScalingKeyCount}");
                        sub.ScalingCount = chan.ScalingKeyCount;
                        sub.ScalingTimes = new float[chan.ScalingKeyCount];
                        sub.ScalingX = new float[chan.ScalingKeyCount];
                        sub.ScalingY = new float[chan.ScalingKeyCount];
                        sub.ScalingZ = new float[chan.ScalingKeyCount];
                        for (var i = 0; i < chan.ScalingKeyCount; i++) {
                            sub.ScalingTimes[i] = (float) chan.ScalingKeys[i].Time;
                            var sca = chan.ScalingKeys[i].Value;
                            sub.ScalingX[i] = sca.X;
                            sub.ScalingY[i] = sca.Y;
                            sub.ScalingZ[i] = sca.Z;
                        }
                    }

                    subAnims.Add(sub);
                }
            }

            debugLog?.Invoke($"Total stored animation channels: {subAnims.Count}");

            return new LiveAnim {
                Bones = mBones.ToArray(),
                BoneSubAnims = subAnims.ToArray()
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
                return JsonSerializer.Deserialize<T>(ms.GetBuffer().AsSpan(0, (int) ms.Length), opts);
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