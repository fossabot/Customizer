using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
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
                using (var writer = new Utf8JsonWriter(stream)) {
                    JsonSerializer.Serialize(writer, definition);
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
        public static LiveContent LiveLoad(ContentDefinition definition, ResourceManager resourceManager,
            LiveLoadOptions opts, CacheManager cacheManager = null) {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));
            if (resourceManager == null)
                throw new ArgumentNullException(nameof(resourceManager));
            definition = (ContentDefinition) definition.Clone();

            // Load meshes
            var meshes = new LiveMesh[definition.MeshPaths.Count];
            for (var i = 0; i < meshes.Length; i++) {
                var path = definition.MeshPaths[i];
                if (opts.UseMeshCache && (meshes[i] = cacheManager?.GetMesh(path)) != null)
                    continue;

                byte[] array;
                using (var stream = resourceManager.GetStream(new Uri(path)))
                using (var ms = new MemoryStream()) {
                    stream.CopyTo(ms);
                    array = ms.ToArray();
                }

                Serializer.Deserialize(array, out meshes[i]);
                if (opts.UseMeshCache)
                    cacheManager?.AddMesh(path, meshes[i]);
            }

            // Load textures
            var baseTextures = new Image<Rgba32>[definition.TexturePaths.Count];
            var renderedCoTextures = new Image<Rgba32>[definition.CoTextures.Count];
            for (var i = 0; i < baseTextures.Length; i++) {
                var path = definition.TexturePaths[i];
                if (opts.UseTextureCache && (baseTextures[i] = cacheManager?.GetTexture(path)) != null)
                    continue;
                using (var stream = resourceManager.GetStream(new Uri(path)))
                    baseTextures[i] =
                        Image.Load<Rgba32>(stream);
                if (opts.UseTextureCache)
                    cacheManager?.AddTexture(path, baseTextures[i]);
            }

            var maxSize = opts.MaxCoTextureSize;
            if (maxSize.Height == 0 || maxSize.Height == 0)
                maxSize = Defaults.Size;
            // Iterate over rendered coalesced textures
            for (var i = 0; i < renderedCoTextures.Length; i++) {
                var conf = definition.CoTextures[i];
                var target = new Image<Rgba32>(Math.Min(conf.Width, maxSize.Width),
                    Math.Min(conf.Height, maxSize.Height));
                renderedCoTextures[i] = target;
                // Iterate over sub-textures
                foreach (var curSub in conf.Textures) {
                    var curSrc = baseTextures[curSub.TextureIdx];
                    var cur = curSub.MaskIdx >= 0
                        ? baseTextures[curSub.MaskIdx].Clone(x => x
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

            // Load sounds
            var sounds = new byte[definition.SoundPaths.Count][];
            for (var i = 0; i < sounds.Length; i++) {
                var path = definition.SoundPaths[i];
                if (opts.UseSoundCache && (sounds[i] = cacheManager?.GetSound(path)) != null)
                    continue;
                byte[] array;
                using (var stream = resourceManager.GetStream(new Uri(path)))
                using (var ms = new MemoryStream()) {
                    stream.CopyTo(ms);
                    array = ms.ToArray();
                }

                sounds[i] = array;
                if (opts.UseSoundCache)
                    cacheManager?.AddSound(path, array);
            }

            // Load translations
            var translations = new Dictionary<string, string>[definition.TranslationPaths.Count];
            for (var i = 0; i < translations.Length; i++) {
                var path = definition.TranslationPaths[i];
                if (opts.UseTranslationCache && (translations[i] = cacheManager?.GetTranslation(path)) != null)
                    continue;
                byte[] array;
                using (var stream = resourceManager.GetStream(new Uri(path)))
                using (var ms = new MemoryStream()) {
                    stream.CopyTo(ms);
                    array = ms.ToArray();
                }

                Serializer.Deserialize(array, out translations[i]);
                if (opts.UseSoundCache)
                    cacheManager?.AddTranslation(path, translations[i]);
            }

            var res = new LiveContent {
                Definition = definition
            };
            res.Meshes.AddRange(meshes);
            res.Textures.AddRange(baseTextures);
            res.RenderedCoTextures.AddRange(renderedCoTextures);
            res.Sounds.AddRange(sounds);
            res.Translations.AddRange(translations);
            return res;
        }

        /// <summary>
        /// Convert float array to 4x4 matrix
        /// </summary>
        /// <param name="m">Array to convert</param>
        /// <returns>Matrix</returns>
        // ReSharper disable InconsistentNaming
        public static Matrix4x4 ToMatrix4x4(this float[] m)
            => m.AsSpan().ToMatrix4x4();


        /// <summary>
        /// Convert span to 4x4 matrix
        /// </summary>
        /// <param name="m">Span to convert</param>
        /// <returns>Matrix</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods",
            Justification = "<Pending>")]
        public static Matrix4x4 ToMatrix4x4(this Span<float> m)
            => new Matrix4x4(m[0], m[1], m[2], m[3], m[4], m[5], m[6], m[7], m[8], m[9], m[10], m[11], m[12], m[13],
                m[14], m[15]);
        // ReSharper restore InconsistentNaming
    }
}