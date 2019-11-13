using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;
using Customizer.Modeling;

namespace Customizer.Content {
    /// <summary>
    /// Stores runtime-loaded content data
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields",
        Justification = "<Pending>")]
    public sealed class LiveContent {
        /// <summary>
        /// Base content definition this instance was loaded from
        /// </summary>
        public ContentDefinition Definition;

        /// <summary>
        /// Map of mesh names to loaded meshes
        /// </summary>
        public Dictionary<string, LiveMesh> Meshes = new Dictionary<string, LiveMesh>();

        /// <summary>
        /// Map of texture names to loaded textures
        /// </summary>
        public Dictionary<string, Image<Rgba32>> Textures = new Dictionary<string, Image<Rgba32>>();

        /// <summary>
        /// Map of composite texture names to loaded composite textures
        /// </summary>
        public Dictionary<string, Image<Rgba32>> RenderedCoTextures = new Dictionary<string, Image<Rgba32>>();

        /// <summary>
        /// Map of resource names to loaded resources
        /// </summary>
        public Dictionary<string, byte[]> Resources = new Dictionary<string, byte[]>();

        /// <summary>
        /// Map of translation names to loaded translations
        /// </summary>
        public Dictionary<string, Dictionary<string, string>> Translations =
            new Dictionary<string, Dictionary<string, string>>();

        /// <summary>
        /// Add or update mesh for this instance
        /// </summary>
        /// <param name="name">Mesh name</param>
        /// <param name="path">Original path of resource</param>
        /// <param name="mesh">Mesh to add</param>
        public void AddMesh(string name, string path, LiveMesh mesh) {
            Definition.MeshPaths[name] = path;
            Meshes[name] = mesh;
        }

        /// <summary>
        /// Remove mesh for this instance
        /// </summary>
        /// <param name="name">Mesh name</param>
        /// <returns>True if successfully removed, false if mesh is referenced in definition and can't be removed</returns>
        public bool RemoveMesh(string name) {
            foreach (var entry in Definition.MeshConfigs.Values)
                if (entry.Mesh == name)
                    return false;
            Definition.MeshPaths.Remove(name);
            Meshes.Remove(name);
            return true;
        }

        /// <summary>
        /// Add or update texture for this instance and update composite textures
        /// </summary>
        /// <param name="name">Texture name</param>
        /// <param name="path">Original path of resource</param>
        /// <param name="image">Texture to add</param>
        public bool AddTexture(string name, string path, Image<Rgba32> image) {
            Definition.TexturePaths[name] = path;
            Textures[name] = image;
            var mod = false;
            foreach (var entry in Definition.CoTextures)
            foreach (var subDef in entry.Value.Textures)
                if (subDef.Mask == name || subDef.Texture == name) {
                    mod = true;
                    RenderedCoTextures[entry.Key] =
                        ContentUtil.CoalesceTexture(Textures, entry.Value, RenderedCoTextures[entry.Key].Size());
                }

            return mod;
        }

        /// <summary>
        /// Remove texture for this instance, checks against co-texture definitions and mesh config material definitions
        /// </summary>
        /// <param name="name">Texture name</param>
        /// <returns>True if successfully removed, false if texture is referenced in definition and can't be removed</returns>
        public bool RemoveTexture(string name) {
            foreach (var entry in Definition.CoTextures.Values)
            foreach (var subdef in entry.Textures)
                if (subdef.Mask == name || subdef.Texture == name)
                    return false;
            foreach (var entry in Definition.MeshConfigs)
            foreach (var sub in entry.Value.Materials)
            foreach (var subsub in sub.Textures)
                if (subsub.Value == name)
                    return false;
            Definition.TexturePaths.Remove(name);
            Textures.Remove(name);
            return true;
        }

        /// <summary>
        /// Add or update resource for this instance
        /// </summary>
        /// <param name="name">Resource name</param>
        /// <param name="path">Original path of resource</param>
        /// <param name="resource">Resource to add</param>
        public void AddResource(string name, string path, byte[] resource) {
            Definition.ResourcePaths[name] = path;
            Resources[name] = resource;
        }

        /// <summary>
        /// Remove resource for this instance
        /// </summary>
        /// <param name="name">Resource name</param>
        public void RemoveResource(string name) {
            Definition.ResourcePaths.Remove(name);
            Resources.Remove(name);
        }

        /// <summary>
        /// Add or update translation for this instance
        /// </summary>
        /// <param name="name">Resource name</param>
        /// <param name="path">Original path of resource</param>
        /// <param name="translation">Translation to add</param>
        public void AddTranslation(string name, string path, Dictionary<string, string> translation) {
            Definition.TranslationPaths[name] = path;
            Translations[name] = translation;
        }

        /// <summary>
        /// Remove translation for this instance
        /// </summary>
        /// <param name="name">Translation name</param>
        public void RemoveTranslation(string name) {
            Definition.TranslationPaths.Remove(name);
            Translations.Remove(name);
        }
    }
}