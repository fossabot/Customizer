using SixLabors.Primitives;

namespace CustomNavi.Utility {
    /// <summary>
    /// Default objects
    /// </summary>
    public static class Defaults {
        /// <summary>
        /// Default texture size
        /// </summary>
        public static readonly Size Size = new Size(1024, 1024);

        /// <summary>
        /// Default content loading options
        /// </summary>
        public static readonly LiveLoadOptions LiveLoadOptions = new LiveLoadOptions(Size);
    }
}