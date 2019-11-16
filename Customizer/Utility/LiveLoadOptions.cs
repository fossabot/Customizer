using SixLabors.Primitives;

namespace Customizer.Utility {
    /// <summary>
    /// Content loading options
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance",
        "CA1815:Override equals and operator equals on value types", Justification = "<Pending>")]
    public struct LiveLoadOptions {
        public LiveLoadOptions(Size? maxCoTextureSize = null, bool? useMeshCache = null, bool? useAnimCache = null,
            bool? useTextureCache = null, bool? useSoundCache = null, bool? useTranslationCache = null,
            bool? loadMeshes = null, bool? loadAnims = null, bool? loadTextures = null, bool? loadResources = null,
            bool? loadTranslations = null) {
            MaxTextureSize = maxCoTextureSize ?? Defaults.Size;
            UseMeshCache = useMeshCache ?? true;
            UseAnimCache = useAnimCache ?? true;
            UseTextureCache = useTextureCache ?? true;
            UseResourceCache = useSoundCache ?? true;
            UseTranslationCache = useTranslationCache ?? true;
            LoadMeshes = loadMeshes ?? true;
            LoadAnims = loadAnims ?? true;
            LoadTextures = loadTextures ?? true;
            LoadResources = loadResources ?? true;
            LoadTranslations = loadTranslations ?? true;
        }

        /// <summary>
        /// Maximum texture size
        /// </summary>
        public Size MaxTextureSize { get; set; }

        /// <summary>
        /// Option to use cache manager for meshes
        /// </summary>
        public bool UseMeshCache { get; set; }

        /// <summary>
        /// Option to use cache manager for animations
        /// </summary>
        public bool UseAnimCache { get; set; }

        /// <summary>
        /// Option to use cache manager for textures
        /// </summary>
        public bool UseTextureCache { get; set; }

        /// <summary>
        /// Option to use cache manager for resources
        /// </summary>
        public bool UseResourceCache { get; set; }

        /// <summary>
        /// Option to use cache manager for translations
        /// </summary>
        public bool UseTranslationCache { get; set; }

        /// <summary>
        /// Option to load meshes
        /// </summary>
        public bool LoadMeshes { get; set; }

        /// <summary>
        /// Option to load animations
        /// </summary>
        public bool LoadAnims { get; set; }

        /// <summary>
        /// Option to load textures
        /// </summary>
        public bool LoadTextures { get; set; }

        /// <summary>
        /// Option to load resources
        /// </summary>
        public bool LoadResources { get; set; }

        /// <summary>
        /// Option to load translations
        /// </summary>
        public bool LoadTranslations { get; set; }
    }
}