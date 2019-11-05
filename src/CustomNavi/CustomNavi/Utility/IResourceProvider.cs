using System;
using System.IO;

namespace CustomNavi.Utility {
    public interface IResourceProvider {
        Stream GetStream(Uri uri);
    }
}