using System;
using System.IO;

namespace Customizer.Utility {
    /// <summary>
    /// Interface for resource stream providers
    /// </summary>
    public interface IDataProvider {
        Stream GetStream(Uri uri);
    }
}