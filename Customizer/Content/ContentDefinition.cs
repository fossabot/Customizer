using System;
using System.Collections.Generic;
using Customizer.Modeling;
using Customizer.Texturing;
using Customizer.Utility;

namespace Customizer.Content {
    /// <summary>
    /// Stores content locations and metadata for a content unit
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
        Justification = "<Pending>")]
    [CzCustomSerializeMembers]
    public sealed class ContentDefinition : ICloneable {
        /// <summary>
        /// Map of mesh names to URIs
        /// </summary>
        [CzSerialize(0)]
        public Dictionary<string, string> MeshPaths { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Map of texture names to URIs
        /// </summary>
        [CzSerialize(1)]
        public Dictionary<string, string> TexturePaths { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Map of resource names to URIs
        /// </summary>
        [CzSerialize(2)]
        public Dictionary<string, string> ResourcePaths { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Map of translation names to URIs
        /// </summary>
        [CzSerialize(3)]
        public Dictionary<string, string> TranslationPaths { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Map of mesh config names to URIs
        /// </summary>
        [CzSerialize(4)]
        public Dictionary<string, MeshConfig> MeshConfigs { get; set; } = new Dictionary<string, MeshConfig>();

        /// <summary>
        /// Map of composite texture names to definitions
        /// </summary>
        [CzSerialize(5)]
        public Dictionary<string, CoTextureDefinition> CoTextures { get; set; } =
            new Dictionary<string, CoTextureDefinition>();

        /// <summary>
        /// Main mesh for this content unit
        /// </summary>
        [CzSerialize(6)]
        public string MainMesh { get; set; }

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