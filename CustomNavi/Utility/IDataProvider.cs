using System;
using System.IO;

namespace CustomNavi.Utility {
    /// <summary>
    /// Interface for resource stream providers
    /// </summary>
    public interface IDataProvider {
        Stream GetStream(Uri uri);
    }
}