using SixLabors.Primitives;

namespace CustomNavi.Utility {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance",
        "CA1815:Override equals and operator equals on value types", Justification = "<Pending>")]
    public struct LiveLoadOptions {
        public LiveLoadOptions(Size? maxCoTextureSize = null, bool? useMeshCache = null, bool? useTextureCache = null,
            bool? useSoundCache = null, bool? useTranslationCache = null) {
            MaxCoTextureSize = maxCoTextureSize ?? Defaults.Size;
            UseMeshCache = useMeshCache ?? false;
            UseTextureCache = useTextureCache ?? false;
            UseSoundCache = useSoundCache ?? false;
            UseTranslationCache = useTranslationCache ?? false;
        }

        public Size MaxCoTextureSize { get; set; }
        public bool UseMeshCache { get; set; }
        public bool UseTextureCache { get; set; }
        public bool UseSoundCache { get; set; }
        public bool UseTranslationCache { get; set; }
    }
}