using SixLabors.Primitives;

namespace CustomNavi.Utility {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance",
        "CA1815:Override equals and operator equals on value types", Justification = "<Pending>")]
    public struct LiveLoadOptions {
        public LiveLoadOptions(Size? maxCoTextureSize = null, bool? useMeshCache = null, bool? useTextureCache = null,
            bool? useSoundCache = null, bool? useTranslationCache = null, bool? loadMeshes = null,
            bool? loadTextures = null, bool? loadResources = null, bool? loadTranslations = null) {
            MaxCoTextureSize = maxCoTextureSize ?? Defaults.Size;
            UseMeshCache = useMeshCache ?? false;
            UseTextureCache = useTextureCache ?? false;
            UseResourceCache = useSoundCache ?? false;
            UseTranslationCache = useTranslationCache ?? false;
            LoadMeshes = loadMeshes ?? false;
            LoadTextures = loadTextures ?? false;
            LoadResources = loadResources ?? false;
            LoadTranslations = loadTranslations ?? false;
        }

        public Size MaxCoTextureSize { get; set; }
        public bool UseMeshCache { get; set; }
        public bool UseTextureCache { get; set; }
        public bool UseResourceCache { get; set; }
        public bool UseTranslationCache { get; set; }
        public bool LoadMeshes { get; set; }
        public bool LoadTextures { get; set; }
        public bool LoadResources { get; set; }
        public bool LoadTranslations { get; set; }
    }
}