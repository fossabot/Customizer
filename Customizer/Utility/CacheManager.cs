using System.Collections.Generic;
using Customizer.Modeling;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Customizer.Utility {
    /// <summary>
    /// Content caching utility
    /// </summary>
    public sealed class CacheManager {
        private readonly Dictionary<string, LiveMesh> _meshes = new Dictionary<string, LiveMesh>();
        private readonly Dictionary<string, Image<Rgba32>> _textures = new Dictionary<string, Image<Rgba32>>();
        private readonly Dictionary<string, byte[]> _resources = new Dictionary<string, byte[]>();

        private readonly Dictionary<string, Dictionary<string, string>> _translations =
            new Dictionary<string, Dictionary<string, string>>();

        /// <summary>
        /// Get cached mesh
        /// </summary>
        /// <param name="path">Path to use</param>
        /// <returns>Cached mesh or null</returns>
        public LiveMesh GetMesh(string path)
            => _meshes.TryGetValue(path, out var res) ? res : null;

        /// <summary>
        /// Add cached mesh
        /// </summary>
        /// <param name="path">Path to use</param>
        /// <param name="mesh">Mesh to add</param>
        public void AddMesh(string path, LiveMesh mesh)
            => _meshes.Add(path, mesh);

        /// <summary>
        /// Get count of cached meshes
        /// </summary>
        /// <returns>Count of cached meshes</returns>
        public int GetMeshCount()
            => _meshes.Count;

        /// <summary>
        /// Get enumerator for meshes by path
        /// </summary>
        /// <returns>Enumerator for meshes by path</returns>
        public Dictionary<string, LiveMesh>.Enumerator GetMeshEnumerator()
            => _meshes.GetEnumerator();

        /// <summary>
        /// Remove cached mesh
        /// </summary>
        /// <param name="path">Path of mesh to remove</param>
        public void RemoveMesh(string path)
            => _meshes.Remove(path);

        /// <summary>
        /// Clear cached meshes
        /// </summary>
        public void ClearMeshes()
            => _meshes.Clear();

        /// <summary>
        /// Get cached texture
        /// </summary>
        /// <param name="path">Path to use</param>
        /// <returns>Cached texture or null</returns>
        public Image<Rgba32> GetTexture(string path)
            => _textures.TryGetValue(path, out var res) ? res : null;


        /// <summary>
        /// Add cached texture
        /// </summary>
        /// <param name="path">Path to use</param>
        /// <param name="texture">Texture to add</param>
        public void AddTexture(string path, Image<Rgba32> texture)
            => _textures.Add(path, texture);

        /// <summary>
        /// Get count of cached textures
        /// </summary>
        /// <returns>Count of cached textures</returns>
        public int GetTextureCount()
            => _textures.Count;

        /// <summary>
        /// Get enumerator for textures by path
        /// </summary>
        /// <returns>Enumerator for textures by path</returns>
        public Dictionary<string, Image<Rgba32>>.Enumerator GetTextureEnumerator()
            => _textures.GetEnumerator();

        /// <summary>
        /// Remove cached texture
        /// </summary>
        /// <param name="path">Path of texture to remove</param>
        public void RemoveTexture(string path)
            => _textures.Remove(path);

        /// <summary>
        /// Clear cached textures
        /// </summary>
        public void ClearTextures()
            => _textures.Clear();

        /// <summary>
        /// Get cached resource
        /// </summary>
        /// <param name="path">Path to use</param>
        /// <returns>Cached resource or null</returns>
        public byte[] GetResource(string path)
            => _resources.TryGetValue(path, out var res) ? res : null;

        /// <summary>
        /// Add cached resource
        /// </summary>
        /// <param name="path">Path to use</param>
        /// <param name="resource">Resource to add</param>
        public void AddResource(string path, byte[] resource)
            => _resources.Add(path, resource);

        /// <summary>
        /// Get count of cached resources
        /// </summary>
        /// <returns>Count of cached resources</returns>
        public int GetResourceCount()
            => _resources.Count;

        /// <summary>
        /// Get enumerator for resources by path
        /// </summary>
        /// <returns>Enumerator for resources by path</returns>
        public Dictionary<string, byte[]>.Enumerator GetResourceEnumerator()
            => _resources.GetEnumerator();

        /// <summary>
        /// Remove cached resource
        /// </summary>
        /// <param name="path">Path of resource to remove</param>
        public void RemoveResource(string path)
            => _resources.Remove(path);

        /// <summary>
        /// Clear cached resources
        /// </summary>
        public void ClearResources()
            => _resources.Clear();

        /// <summary>
        /// Get cached translation
        /// </summary>
        /// <param name="path">Path to use</param>
        /// <returns>Cached translation or null</returns>
        public Dictionary<string, string> GetTranslation(string path)
            => _translations.TryGetValue(path, out var res) ? res : null;

        /// <summary>
        /// Add cached translation
        /// </summary>
        /// <param name="path">Path to use</param>
        /// <param name="translation">Translation to add</param>
        public void AddTranslation(string path, Dictionary<string, string> translation)
            => _translations.Add(path, translation);

        /// <summary>
        /// Get count of cached translations
        /// </summary>
        /// <returns>Count of cached translations</returns>
        public int GetTranslationCount()
            => _translations.Count;

        /// <summary>
        /// Get enumerator for translations by path
        /// </summary>
        /// <returns>Enumerator for translations by path</returns>
        public Dictionary<string, Dictionary<string, string>>.Enumerator GetTranslationEnumerator()
            => _translations.GetEnumerator();

        /// <summary>
        /// Remove cached translation
        /// </summary>
        /// <param name="path">Path of translation to remove</param>
        public void RemoveTranslation(string path)
            => _translations.Remove(path);

        /// <summary>
        /// Clear cached translations
        /// </summary>
        public void ClearTranslations()
            => _translations.Clear();
    }
}