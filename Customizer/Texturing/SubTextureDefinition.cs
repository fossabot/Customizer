namespace Customizer.Texturing {
    /// <summary>
    /// Definition of layer of a composite texture
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance",
        "CA1815:Override equals and operator equals on value types", Justification = "<Pending>")]
    public struct SubTextureDefinition {
        /// <summary>
        /// Texture name
        /// </summary>
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public string Texture { get; set; }

        /// <summary>
        /// Mask name
        /// </summary>
        public string Mask { get; set; }
        // ReSharper restore UnusedAutoPropertyAccessor.Global
    }
}