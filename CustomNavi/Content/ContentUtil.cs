using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using CustomNavi.Modeling;
using CustomNavi.Texturing;
using CustomNavi.Utility;
using SixLabors.Primitives;

namespace CustomNavi.Content {
    /// <summary>
    /// Content loading utility
    /// </summary>
    public static class ContentUtil {
        /// <summary>
        /// Load live data representation of content
        /// </summary>
        /// <param name="definition">Definition to load assets from</param>
        /// <param name="dataManager">Resource manager to load assets with</param>
        /// <param name="opts">Loader options</param>
        /// <param name="cacheManager">Optional content cache manager</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"><paramref name="definition"/> or <paramref name="dataManager"/> are null</exception>
        // ReSharper disable once UnusedMember.Global
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability",
            "CA2000: DisposeObjectsBeforeLosingScope", Justification = "<Pending>")]
        public static LiveContent LoadLiveContent(ContentDefinition definition, DataManager dataManager,
            LiveLoadOptions opts, CacheManager cacheManager = null) {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));
            if (dataManager == null)
                throw new ArgumentNullException(nameof(dataManager));
            definition = (ContentDefinition) definition.Clone();

            // Load meshes
            var meshes = new Dictionary<string, LiveMesh>();
            if (opts.LoadMeshes) {
                foreach (var e in definition.MeshPaths) {
                    var path = new Uri(e.Value);
                    LiveMesh mesh;
                    if (!opts.UseMeshCache || (mesh = cacheManager?.GetMesh(path.AbsolutePath)) == null) {
                        using (var stream = dataManager.GetStream(path))
                            mesh = Serializer.Deserialize<LiveMesh>(stream);

                        if (opts.UseMeshCache)
                            cacheManager?.AddMesh(path.AbsolutePath, mesh);
                    }

                    meshes.Add(e.Key, mesh);
                }
            }

            // Load textures
            var baseTextures = new Dictionary<string, Image<Rgba32>>();
            var renderedCoTextures = new Dictionary<string, Image<Rgba32>>();
            if (opts.LoadTextures) {
                var maxSize = opts.MaxTextureSize;
                if (maxSize.Height == 0 || maxSize.Height == 0)
                    maxSize = Defaults.Size;
                foreach (var e in definition.TexturePaths) {
                    var path = new Uri(e.Value);
                    Image<Rgba32> tex;
                    if (!opts.UseTextureCache || (tex = cacheManager?.GetTexture(path.AbsolutePath)) == null) {
                        using (var stream = dataManager.GetStream(path))
                            tex = Image.Load<Rgba32>(stream);
                        tex.Mutate(x => x.Resize(new Size(Math.Min(tex.Width, maxSize.Width),
                            Math.Min(tex.Height, maxSize.Height))));
                        if (opts.UseTextureCache)
                            cacheManager?.AddTexture(path.AbsolutePath, tex);
                    }

                    baseTextures.Add(e.Key, tex);
                }


                // Iterate over rendered coalesced textures
                foreach (var e in definition.CoTextures) {
                    renderedCoTextures.Add(e.Key,
                        CoalesceTexture(baseTextures, e.Value,
                            new Size(Math.Min(e.Value.Width, maxSize.Width),
                                Math.Min(e.Value.Height, maxSize.Height))));
                }
            }

            // Load resources
            var resources = new Dictionary<string, byte[]>();
            if (opts.LoadResources) {
                foreach (var e in definition.ResourcePaths) {
                    var path = new Uri(e.Value);
                    byte[] resource;
                    if (!opts.UseResourceCache || (resource = cacheManager?.GetResource(path.AbsolutePath)) == null) {
                        using (var stream = dataManager.GetStream(path))
                        using (var ms = new MemoryStream()) {
                            stream.CopyTo(ms);
                            resource = ms.ToArray();
                        }

                        if (opts.UseResourceCache)
                            cacheManager?.AddResource(path.AbsolutePath, resource);
                    }

                    resources.Add(e.Key, resource);
                }
            }

            // Load translations
            var translations = new Dictionary<string, Dictionary<string, string>>();
            if (opts.LoadTranslations) {
                foreach (var e in definition.TranslationPaths) {
                    var path = new Uri(e.Value);
                    Dictionary<string, string> translation;
                    if (!opts.UseTranslationCache ||
                        (translation = cacheManager?.GetTranslation(path.AbsolutePath)) == null) {
                        using (var stream = dataManager.GetStream(path))
                            translation = Serializer.Deserialize<Dictionary<string, string>>(stream);

                        if (opts.UseTranslationCache)
                            cacheManager?.AddTranslation(path.AbsolutePath, translation);
                    }

                    translations.Add(e.Key, translation);
                }
            }

            return new LiveContent {
                Definition = definition,
                Meshes = meshes,
                Textures = baseTextures,
                RenderedCoTextures = renderedCoTextures,
                Resources = resources,
                Translations = translations
            };
        }

        /// <summary>
        /// Render composite texture
        /// </summary>
        /// <param name="textures">Component textures</param>
        /// <param name="definition">Composite sequence definition</param>
        /// <param name="targetSize">Target output size</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Image<Rgba32> CoalesceTexture(Dictionary<string, Image<Rgba32>> textures,
            CoTextureDefinition definition, Size targetSize) {
            if (textures == null)
                throw new ArgumentNullException(nameof(textures));
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));
            var target = new Image<Rgba32>(targetSize.Width, targetSize.Height);
            // Iterate over sub-textures
            foreach (var curSub in definition.Textures) {
                var curSrc = textures[curSub.Texture];
                Image<Rgba32> cur = null;
                try {
                    if (curSub.Mask != null) {
                        // Sub-texture is masked, resize texture to mask, mask texture, resize masked texture to target
                        var curMsk = textures[curSub.Mask];
                        using (var curSrcResized = curSrc.Clone(x => x.Resize(curMsk.Size())))
                            cur = curMsk.Clone(x => x
                                // ReSharper disable once AccessToDisposedClosure
                                .DrawImage(curSrcResized,
                                    new GraphicsOptions {AlphaCompositionMode = PixelAlphaCompositionMode.SrcIn})
                                .Resize(target.Size())
                            );
                    }
                    else
                        // Sub-texture is not masked, resize texture to target
                        cur = curSrc.Clone(x => x
                            .Resize(target.Size())
                        );

                    // ReSharper disable once AccessToDisposedClosure
                    target.Mutate(x => x.DrawImage(cur, 1.0f));
                }
                finally {
                    cur?.Dispose();
                }
            }

            return target;
        }
    }
}