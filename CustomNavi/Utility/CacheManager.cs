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
        private readonly Dictionary<string, byte[]> _sounds = new Dictionary<string, byte[]>();

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

        public byte[] GetSound(string path)
            => _sounds.TryGetValue(path, out var res) ? res : null;

        public void AddSound(string path, byte[] sound)
            => _sounds.Add(path, sound);

        public int GetSoundCount()
            => _sounds.Count;

        public Dictionary<string, byte[]>.Enumerator GetSoundEnumerator()
            => _sounds.GetEnumerator();

        public void RemoveSound(string path)
            => _sounds.Remove(path);

        public void ClearSounds()
            => _sounds.Clear();

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