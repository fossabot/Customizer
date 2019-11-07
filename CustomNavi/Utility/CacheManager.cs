using System.Collections.Generic;
using CustomNavi.Modeling;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CustomNavi.Utility {
    /// <summary>
    /// Content caching utility
    /// </summary>
    public class CacheManager {
        private readonly Dictionary<string, LiveMesh> _meshes = new Dictionary<string, LiveMesh>();
        private readonly Dictionary<string, Image<Rgba32>> _textures = new Dictionary<string, Image<Rgba32>>();
        private readonly Dictionary<string, byte[]> _resources = new Dictionary<string, byte[]>();

        private readonly Dictionary<string, Dictionary<string, string>> _translations =
            new Dictionary<string, Dictionary<string, string>>();

        public LiveMesh GetMesh(string path)
            => _meshes.TryGetValue(path, out var res) ? res : null;

        public void AddMesh(string path, LiveMesh mesh)
            => _meshes.Add(path, mesh);

        public int GetMeshCount()
            => _meshes.Count;

        public Dictionary<string, LiveMesh>.Enumerator GetMeshEnumerator()
            => _meshes.GetEnumerator();

        public void RemoveMesh(string path)
            => _meshes.Remove(path);

        public void ClearMeshes()
            => _meshes.Clear();

        public Image<Rgba32> GetTexture(string path)
            => _textures.TryGetValue(path, out var res) ? res : null;

        public void AddTexture(string path, Image<Rgba32> texture)
            => _textures.Add(path, texture);

        public int GetTextureCount()
            => _textures.Count;

        public Dictionary<string, Image<Rgba32>>.Enumerator GetTextureEnumerator()
            => _textures.GetEnumerator();

        public void RemoveTexture(string path)
            => _textures.Remove(path);

        public void ClearTextures()
            => _textures.Clear();

        public byte[] GetResource(string path)
            => _resources.TryGetValue(path, out var res) ? res : null;

        public void AddResource(string path, byte[] resource)
            => _resources.Add(path, resource);

        public int GetResourceCount()
            => _resources.Count;

        public Dictionary<string, byte[]>.Enumerator GetResourceEnumerator()
            => _resources.GetEnumerator();

        public void RemoveResource(string path)
            => _resources.Remove(path);

        public void ClearResources()
            => _resources.Clear();

        public Dictionary<string, string> GetTranslation(string path)
            => _translations.TryGetValue(path, out var res) ? res : null;

        public void AddTranslation(string path, Dictionary<string, string> translation)
            => _translations.Add(path, translation);

        public int GetTranslationCount()
            => _translations.Count;

        public Dictionary<string, Dictionary<string, string>>.Enumerator GetTranslationEnumerator()
            => _translations.GetEnumerator();

        public void RemoveTranslation(string path)
            => _translations.Remove(path);

        public void ClearTranslations()
            => _translations.Clear();
    }
}