using CustomNavi.Modeling;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;

namespace CustomNavi.Content {
    /// <summary>
    /// Stores runtime-usable content data
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields",
        Justification = "<Pending>")]
    public class LiveContent : IDisposable {
        public ContentDefinition Definition;
        public Dictionary<string, LiveMesh> Meshes = new Dictionary<string, LiveMesh>();
        public Dictionary<string, Image<Rgba32>> Textures = new Dictionary<string, Image<Rgba32>>();
        public Dictionary<string, Image<Rgba32>> RenderedCoTextures = new Dictionary<string, Image<Rgba32>>();
        public Dictionary<string, byte[]> Resources = new Dictionary<string, byte[]>();

        public Dictionary<string, Dictionary<string, string>> Translations =
            new Dictionary<string, Dictionary<string, string>>();

        public void AddMesh(string name, string path, LiveMesh mesh) {
            Definition.MeshPaths[name] = path;
            Meshes[name] = mesh;
        }

        public bool RemoveMesh(string name) {
            foreach (var entry in Definition.MeshConfigs.Values)
                if (entry.Mesh == name)
                    return false;
            Definition.MeshPaths.Remove(name);
            Meshes.Remove(name);
            return true;
        }

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

        public bool RemoveTexture(string name) {
            foreach (var entry in Definition.CoTextures.Values)
            foreach (var subdef in entry.Textures)
                if (subdef.Mask == name || subdef.Texture == name)
                    return false;
            Definition.TexturePaths.Remove(name);
            Textures.Remove(name);
            return true;
        }

        public void AddResource(string name, string path, byte[] resource) {
            Definition.ResourcePaths[name] = path;
            Resources[name] = resource;
        }

        public void RemoveResource(string name) {
            Definition.ResourcePaths.Remove(name);
            Resources.Remove(name);
        }

        public void AddTranslation(string name, string path, Dictionary<string, string> translation) {
            Definition.TranslationPaths[name] = path;
            Translations[name] = translation;
        }

        public void RemoveTranslation(string name) {
            Definition.TranslationPaths.Remove(name);
            Translations.Remove(name);
        }

        // TODO content update functions

        #region IDisposable Support

        private bool _disposed; // To detect redundant calls

        protected virtual void Dispose(bool disposing) {
            if (_disposed) return;
            Definition = null;
            Meshes = null;
            Textures = null;
            RenderedCoTextures = null;
            Resources = null;
            Translations = null;

            _disposed = true;
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose() {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}