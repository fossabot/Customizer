using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CustomNavi.Utility {
    /// <summary>
    /// Content loading utility
    /// </summary>
    public class DataManager {
        private readonly HashSet<IDataProvider> _providers = new HashSet<IDataProvider>();
        private readonly HashSet<IDataProvider> _stdProviders = new HashSet<IDataProvider>();

        public DataManager() {
            _stdProviders.Add(new HexProcDataProvider());
        }

        /// <summary>
        /// Register provider for use
        /// </summary>
        /// <param name="provider">Data provider to add</param>
        public void RegisterDataProvider(IDataProvider provider)
            => _providers.Add(provider);

        /// <summary>
        /// Deregister provider
        /// </summary>
        /// <param name="provider">Data provider to remove</param>
        public void DeregisterDataProvider(IDataProvider provider)
            => _providers.Remove(provider);

        /// <summary>
        /// Attempt to load stream for given uri
        /// </summary>
        /// <param name="uri">Location of resource</param>
        /// <returns>Stream or null</returns>
        /// <exception cref="ArgumentNullException"><paramref name="uri"/> is null</exception>
        public Stream GetStream(Uri uri) {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            foreach (var provider in _stdProviders) {
                var res = provider.GetStream(uri);
                if (res != null)
                    return GetModifiedStream(res, uri);
            }

            foreach (var provider in _providers) {
                var res = provider.GetStream(uri);
                if (res != null)
                    return GetModifiedStream(res, uri);
            }

            return null;
        }

        /// <summary>
        /// Create appropriate stream based on uri scheme
        /// </summary>
        /// <param name="stream">Base content stream</param>
        /// <param name="uri">URI to use</param>
        /// <returns>Stream chosen from URI scheme</returns>
        private static Stream GetModifiedStream(Stream stream, Uri uri) =>
            "deflate".Equals(uri.Scheme, StringComparison.InvariantCultureIgnoreCase)
                ? new DeflateStream(stream, CompressionMode.Decompress)
                : stream;

        /// <summary>
        /// Procedural resource provider (hex colors)
        /// </summary>
        private class HexProcDataProvider : IDataProvider {
            public Stream GetStream(Uri uri) {
                if (uri == null)
                    throw new ArgumentNullException(nameof(uri));
                if (!"proc".Equals(uri.Host, StringComparison.InvariantCultureIgnoreCase)) return null;
                if (!uint.TryParse(
                    uri.AbsolutePath.Trim(Path.VolumeSeparatorChar, Path.DirectorySeparatorChar,
                        Path.AltDirectorySeparatorChar), NumberStyles.HexNumber, null, out var res)) return null;
                var resStream = new MemoryStream();
                using (var img = new Image<Rgba32>(1, 1)
                    {[0, 0] = new Rgba32((byte) (res >> 16), (byte) (res >> 8), (byte) res)})
                    img.SaveAsPng(resStream);
                resStream.Position = 0;
                return resStream;
            }
        }
    }
}