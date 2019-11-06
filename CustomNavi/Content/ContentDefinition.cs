using CustomNavi.Modeling;
using CustomNavi.Texturing;
using System;
using System.Collections.Generic;
using CustomNavi.Utility;

namespace CustomNavi.Content {
    /// <summary>
    /// Stores content locations and metadata
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
        Justification = "<Pending>")]
    [NCustomSerializeMembers]
    public class ContentDefinition : ICloneable {
        [NSerialize(0)] public List<string> MeshPaths { get; set; } = new List<string>();
        [NSerialize(1)] public List<string> TexturePaths { get; set; } = new List<string>();
        [NSerialize(2)] public List<string> SoundPaths { get; set; } = new List<string>();
        [NSerialize(3)] public List<string> TranslationPaths { get; set; } = new List<string>();
        [NSerialize(4)] public List<MeshConfig> MeshConfigs { get; set; } = new List<MeshConfig>();
        [NSerialize(5)] public List<CoTextureDefinition> CoTextures { get; set; } = new List<CoTextureDefinition>();

        public object Clone() {
            var res = new ContentDefinition();
            res.MeshPaths.AddRange(MeshPaths);
            foreach (var e in MeshConfigs)
                res.MeshConfigs.Add((MeshConfig) e.Clone());
            res.TexturePaths.AddRange(TexturePaths);
            res.SoundPaths.AddRange(SoundPaths);
            res.TranslationPaths.AddRange(TranslationPaths);
            foreach (var e in CoTextures)
                res.CoTextures.Add((CoTextureDefinition) e.Clone());
            return res;
        }
    }
}