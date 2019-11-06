using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace CustomNavi.Utility {
    /// <summary>
    /// Content loading utility
    /// </summary>
    public class ResourceManager {
        private readonly HashSet<IResourceProvider> _providers = new HashSet<IResourceProvider>();

        /// <summary>
        /// Register provider for use
        /// </summary>
        /// <param name="provider">Resource provider to add</param>
        public void RegisterResourceProvider(IResourceProvider provider)
            => _providers.Add(provider);

        /// <summary>
        /// Deregister provider
        /// </summary>
        /// <param name="provider">Resource provider to remove</param>
        public void DeregisterResourceProvider(IResourceProvider provider)
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
            if ("builtin".Equals(uri.Host, StringComparison.InvariantCultureIgnoreCase)) {
                var res = typeof(ResourceManager).Assembly.GetManifestResourceStream(uri.AbsolutePath);
                if (res != null)
                    return "deflate".Equals(uri.Scheme, StringComparison.InvariantCultureIgnoreCase)
                        ? new DeflateStream(res, CompressionMode.Decompress)
                        : res;
            }

            foreach (var provider in _providers) {
                var res = provider.GetStream(uri);
                if (res != null)
                    return "deflate".Equals(uri.Scheme, StringComparison.InvariantCultureIgnoreCase)
                        ? new DeflateStream(res, CompressionMode.Decompress)
                        : res;
            }

            return null;
        }
    }
}