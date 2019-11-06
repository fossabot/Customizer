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
        public List<LiveMesh> Meshes = new List<LiveMesh>();
        public List<Image<Rgba32>> Textures = new List<Image<Rgba32>>();
        public List<Image<Rgba32>> RenderedCoTextures = new List<Image<Rgba32>>();
        public List<byte[]> Sounds = new List<byte[]>();
        public List<Dictionary<string, string>> Translations = new List<Dictionary<string, string>>();

        // TODO content update functions

        #region IDisposable Support

        private bool _disposed; // To detect redundant calls

        protected virtual void Dispose(bool disposing) {
            if (_disposed) return;
            Definition = null;
            Meshes = null;
            Textures = null;
            RenderedCoTextures = null;
            Sounds = null;
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