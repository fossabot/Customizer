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
        [NSerialize(0)] public Dictionary<string, string> MeshPaths { get; set; } = new Dictionary<string, string>();
        [NSerialize(1)] public Dictionary<string, string> TexturePaths { get; set; } = new Dictionary<string, string>();
        [NSerialize(2)] public Dictionary<string, string> ResourcePaths { get; set; } = new Dictionary<string, string>();

        [NSerialize(3)]
        public Dictionary<string, string> TranslationPaths { get; set; } = new Dictionary<string, string>();

        [NSerialize(4)]
        public Dictionary<string, MeshConfig> MeshConfigs { get; set; } = new Dictionary<string, MeshConfig>();

        [NSerialize(5)]
        public Dictionary<string, CoTextureDefinition> CoTextures { get; set; } =
            new Dictionary<string, CoTextureDefinition>();

        public object Clone() {
            var res = new ContentDefinition();
            foreach (var e in MeshPaths)
                res.MeshPaths.Add(e.Key, e.Value);
            foreach (var e in MeshConfigs)
                res.MeshConfigs.Add(e.Key, (MeshConfig) e.Value.Clone());
            foreach (var e in TexturePaths)
                res.TexturePaths.Add(e.Key, e.Value);
            foreach (var e in ResourcePaths)
                res.ResourcePaths.Add(e.Key, e.Value);
            foreach (var e in TranslationPaths)
                res.TranslationPaths.Add(e.Key, e.Value);
            foreach (var e in CoTextures)
                res.CoTextures.Add(e.Key, (CoTextureDefinition) e.Value.Clone());
            return res;
        }
    }
}