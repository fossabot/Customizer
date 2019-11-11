using System;
using System.IO;

namespace CustomNavi.Utility {
    /// <summary>
    /// Resource provider based on files in directory
    /// </summary>
    public class FileDataProvider : IDataProvider {
        private readonly string _basePath;

        public FileDataProvider(string basePath) {
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