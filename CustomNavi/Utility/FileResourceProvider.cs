using System;
using System.IO;

namespace CustomNavi.Utility {
    public class FileResourceProvider : IResourceProvider {
        private readonly string _basePath;

        public FileResourceProvider(string basePath) {
            if (basePath == null)
                throw new ArgumentNullException(nameof(basePath));
            _basePath = basePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        public Stream GetStream(Uri uri) {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));
            if (!"local".Equals(uri.Host, StringComparison.InvariantCultureIgnoreCase)) return null;
            var info = new FileInfo(_basePath + uri.AbsolutePath);
            return !info.Exists ? null : info.OpenRead();
        }
    }
}