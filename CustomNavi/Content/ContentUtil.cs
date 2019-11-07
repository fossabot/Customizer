using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using CustomNavi.Modeling;
using CustomNavi.Utility;

namespace CustomNavi.Content {
    /// <summary>
    /// Utility class
    /// </summary>
    public static class ContentUtil {
        /// <summary>
        /// Deserialize content definition from json or binary
        /// </summary>
        /// <param name="span">Span to read from</param>
        /// <param name="json">If true, deserialize from json</param>
        /// <returns>Deserialized definition</returns>
        public static ContentDefinition DeserializeContentDefinition(Span<byte> span, bool json = true)
            => json
                ? JsonSerializer.Deserialize<ContentDefinition>(span)
                : Serializer.Deserialize<ContentDefinition>(span);

        /// <summary>
        /// Serialize content definition to json or binary
        /// </summary>
        /// <param name="definition">Definition to serialize</param>
        /// <param name="stream">Stream to write to</param>
        /// <param name="json">If true, serialize to json</param>
        /// <exception cref="ArgumentNullException"><paramref name="definition"/> or <paramref name="stream"/> are null</exception>
        public static void SerializeContentDefinition(ContentDefinition definition, Stream stream, bool json = true) {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (json)
                using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions {Indented = true})) {
                    JsonSerializer.Serialize(writer, definition, new JsonSerializerOptions {WriteIndented = true});
                }
            else
                Serializer.Serialize(stream, definition);
        }

        /// <summary>
        /// Load live data representation of content
        /// </summary>
        /// <param name="definition">Definition to load assets from</param>
        /// <param name="resourceManager">Resource manager to load assets with</param>
        /// <param name="opts">Loader options</param>
        /// <param name="cacheManager">Optional content cache manager</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"><paramref name="definition"/> or <paramref name="resourceManager"/> are null</exception>
        /// <exception cref="Exception"></exception>
        // ReSharper disable once UnusedMember.Global
        public static LiveContent LoadLiveContent(ContentDefinition definition, ResourceManager resourceManager,
            LiveLoadOptions opts, CacheManager cacheManager = null) {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));
            if (resourceManager == null)
                throw new ArgumentNullException(nameof(resourceManager));
            definition = (ContentDefinition) definition.Clone();

            // Load meshes
            var meshes = new Dictionary<string, LiveMesh>();
            foreach (var e in definition.MeshPaths) {
                var path = e.Value;
                LiveMesh mesh;
                if (!opts.UseMeshCache || (mesh = cacheManager?.GetMesh(path)) == null) {
                    byte[] array;
                    using (var stream = resourceManager.GetStream(new Uri(path)))
                    using (var ms = new MemoryStream()) {
                        stream.CopyTo(ms);
                        array = ms.ToArray();
                    }

                    Serializer.Deserialize(array, out mesh);
                    if (opts.UseMeshCache)
                        cacheManager?.AddMesh(path, mesh);
                }

                meshes.Add(e.Key, mesh);
            }

            // Load textures
            var baseTextures = new Dictionary<string, Image<Rgba32>>();
            var renderedCoTextures = new Dictionary<string, Image<Rgba32>>();
            foreach (var e in definition.TexturePaths) {
                var path = e.Value;
                Image<Rgba32> tex;
                if (!opts.UseTextureCache || (tex = cacheManager?.GetTexture(path)) == null) {
                    using (var stream = resourceManager.GetStream(new Uri(path)))
                        tex = Image.Load<Rgba32>(stream);
                    if (opts.UseTextureCache)
                        cacheManager?.AddTexture(path, tex);
                }

                baseTextures.Add(e.Key, tex);
            }

            var maxSize = opts.MaxCoTextureSize;
            if (maxSize.Height == 0 || maxSize.Height == 0)
                maxSize = Defaults.Size;
            // Iterate over rendered coalesced textures
            foreach (var e in definition.CoTextures) {
                var conf = e.Value;
                var target = new Image<Rgba32>(Math.Min(conf.Width, maxSize.Width),
                    Math.Min(conf.Height, maxSize.Height));
                renderedCoTextures.Add(e.Key, target);
                // Iterate over sub-textures
                foreach (var curSub in conf.Textures) {
                    var curSrc = baseTextures[curSub.Texture];
                    var cur = curSub.Mask != null
                        ? baseTextures[curSub.Mask].Clone(x => x
                            .Resize(curSrc.Size())
                            .DrawImage(curSrc,
                                new GraphicsOptions {AlphaCompositionMode = PixelAlphaCompositionMode.SrcIn})
                            .Resize(target.Size())
                        )
                        : curSrc.Clone(x => x
                            .Resize(target.Size())
                        );
                    try {
                        // ReSharper disable once AccessToDisposedClosure
                        target.Mutate(x => x.DrawImage(cur, 1.0f));
                    }
                    finally {
                        cur?.Dispose();
                    }
                }
            }

            // Load resources
            var resources = new Dictionary<string, byte[]>();
            foreach (var e in definition.ResourcePaths) {
                var path = e.Value;
                byte[] resource;
                if (!opts.UseSoundCache || (resource = cacheManager?.GetResource(path)) == null) {
                    using (var stream = resourceManager.GetStream(new Uri(path)))
                    using (var ms = new MemoryStream()) {
                        stream.CopyTo(ms);
                        resource = ms.ToArray();
                    }

                    if (opts.UseSoundCache)
                        cacheManager?.AddResource(path, resource);
                }

                resources.Add(e.Key, resource);
            }

            // Load translations
            var translations = new Dictionary<string, Dictionary<string, string>>();
            foreach (var e in definition.TranslationPaths) {
                var path = e.Value;
                Dictionary<string, string> translation;
                if (!opts.UseTranslationCache || (translation = cacheManager?.GetTranslation(path)) == null) {
                    byte[] array;
                    using (var stream = resourceManager.GetStream(new Uri(path)))
                    using (var ms = new MemoryStream()) {
                        stream.CopyTo(ms);
                        array = ms.ToArray();
                    }

                    Serializer.Deserialize(array, out translation);
                    if (opts.UseSoundCache)
                        cacheManager?.AddTranslation(path, translation);
                }

                translations.Add(e.Key, translation);
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
    }
}