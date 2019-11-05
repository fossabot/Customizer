using System;
using System.IO;
using System.IO.Compression;

namespace CustomNavi.Utility {
    public class FileResourceProvider : IResourceProvider {
        public Stream GetStream(Uri uri) {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));
            if (!"local".Equals(uri.Host, StringComparison.InvariantCultureIgnoreCase)) return null;
            var info = new FileInfo(uri.AbsolutePath);
            if (!info.Exists)
                return null;
            var res = info.OpenRead();
            return "deflate".Equals(uri.Scheme, StringComparison.InvariantCultureIgnoreCase)
                ? (Stream) new DeflateStream(res, CompressionMode.Decompress)
                : res;
        }
    }
}