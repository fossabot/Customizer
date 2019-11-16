using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Customizer.Utility {
    /// <summary>
    /// Content loading utility
    /// </summary>
    public sealed class DataManager {
        private readonly HashSet<IDataProvider> _providers = new HashSet<IDataProvider>();
        private readonly HashSet<IDataProvider> _stdProviders = new HashSet<IDataProvider>();

        public DataManager() {
            _stdProviders.Add(new ProcDataProvider());
        }

        /// <summary>
        /// Register provider for use by data manager
        /// </summary>
        /// <param name="provider">Data provider to add</param>
        public void RegisterDataProvider(IDataProvider provider)
            => _providers.Add(provider);

        /// <summary>
        /// Deregister provider from data manager
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
        public Stream GetStream(string uri)
            => GetStream(new Uri(uri));

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
        private static Stream GetModifiedStream(Stream stream, Uri uri) {
            switch (uri.Scheme.ToUpperInvariant()) {
                case "FILE":
                    return stream;
                case "DEFLATE":
                    return new DeflateStream(stream, CompressionMode.Decompress);
                default:
                    return stream;
            }
        }

        /// <summary>
        /// Procedural resource provider (hex colors)
        /// </summary>
        private class ProcDataProvider : IDataProvider {
            public Stream GetStream(Uri uri) {
                if (uri == null)
                    throw new ArgumentNullException(nameof(uri));
                if (!"proc".Equals(uri.Host, StringComparison.InvariantCultureIgnoreCase)) return null;
                var match = Regex.Match(uri.AbsolutePath, @"\/([A-Za-z]*):(.*)");
                if (!match.Success) return null;
                switch (match.Groups[1].Value.ToUpperInvariant()) {
                    case "COLOR":
                        if (!uint.TryParse(match.Groups[2].Value.TrimEnd('/'), NumberStyles.HexNumber, null,
                            out var res))
                            return null;
                        var resStream = new MemoryStream();
                        using (var img = new Image<Rgba32>(1, 1)
                            {[0, 0] = new Rgba32((byte) (res >> 16), (byte) (res >> 8), (byte) res)})
                            img.SaveAsPng(resStream);
                        resStream.Position = 0;
                        return resStream;
                    default:
                        return null;
                }
            }
        }
    }
}