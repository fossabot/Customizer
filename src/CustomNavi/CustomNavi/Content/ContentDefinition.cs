using CustomNavi.Modeling;
using CustomNavi.Texturing;
using System;
using System.Collections.Generic;

namespace CustomNavi.Content {
    /// <summary>
    /// Stores content locations and metadata
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
        Justification = "<Pending>")]
    public class ContentDefinition : ICloneable {
        public List<string> MeshPaths { get; set; } = new List<string>();
        public List<MeshConfig> MeshConfigs { get; set; } = new List<MeshConfig>();
        public List<string> TexturePaths { get; set; } = new List<string>();
        public List<string> SoundPaths { get; set; } = new List<string>();
        public List<string> TranslationPaths { get; set; } = new List<string>();
        public List<CoTextureDefinition> CoTextures { get; set; } = new List<CoTextureDefinition>();

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