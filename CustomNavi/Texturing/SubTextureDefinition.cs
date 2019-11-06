namespace CustomNavi.Texturing {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance",
        "CA1815:Override equals and operator equals on value types", Justification = "<Pending>")]
    public struct SubTextureDefinition {
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public int TextureIdx { get; set; }
        public int MaskIdx { get; set; }
        // ReSharper restore UnusedAutoPropertyAccessor.Global
    }
}