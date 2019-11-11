using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Customizer.Content;
using Customizer.Utility;

namespace Customizer.Box {
    /// <summary>
    /// Base content container
    /// </summary>
    public class CzBox {
        private Stream _stream;
        private ContentDefinition _contentDefinition;
        private Dictionary<string, Tuple<int, int>> _entries;
        private long _bOfs;

        /// <summary>
        /// Write container header information for given data to stream
        /// </summary>
        /// <param name="stream">Stream to write to</param>
        /// <param name="contentDefinition">Content definition to serialize (can be null)</param>
        /// <param name="entries">Entry information to serialize (can be null)</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is null</exception>
        public static void WriteHead(Stream stream, ContentDefinition contentDefinition,
            List<MutableKeyValuePair<string, int>> entries) {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            using (var ms = new MemoryStream()) {
                using (var ds = new DeflateStream(ms, CompressionLevel.Optimal, true)) {
                    Serializer.Serialize(ds, contentDefinition);
                    Serializer.Serialize(ds, entries);
                }

                stream.Write7B(ms.Length);
                ms.Position = 0;
                ms.CopyTo(stream);
            }
        }

        /// <summary>
        /// Load container information from stream
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <returns>Container object based on input stream</returns>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is null</exception>
        public static CzBox Load(Stream stream) {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            var baseOff = stream.Read7B(out var sbLen);
            var res = new CzBox {
                _stream = stream, _entries = new Dictionary<string, Tuple<int, int>>(),
                _bOfs = baseOff + sbLen
            };
            using (var ds = new DeflateStream(stream, CompressionMode.Decompress, true)) {
                res._contentDefinition = Serializer.Deserialize<ContentDefinition>(ds);
                var ofs = 0;
                var list = Serializer.Deserialize<List<MutableKeyValuePair<string, int>>>(ds);
                if (list != null)
                    foreach (var (offset, length) in list) {
                        res._entries.Add(offset, new Tuple<int, int>(ofs, length));
                        ofs += length;
                    }
            }

            return res;
        }

        /// <summary>
        /// Get ContentDefinition from container if it contains one
        /// </summary>
        /// <returns>ContentDefinition obtained from container or null</returns>
        public ContentDefinition GetContentDefinition()
            => (ContentDefinition) _contentDefinition?.Clone();

        /// <summary>
        /// Load live data representation of content
        /// </summary>
        /// <param name="dataManager">Resource manager to load assets with</param>
        /// <param name="opts">Loader options</param>
        /// <param name="cacheManager">Optional content cache manager</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"><paramref name="dataManager"/> is null</exception>
        /// <exception cref="InvalidOperationException">content definition was not loaded for this object</exception>
        // ReSharper disable once UnusedMember.Global
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability",
            "CA2000: DisposeObjectsBeforeLosingScope", Justification = "<Pending>")]
        public LiveContent LoadLiveContent(DataManager dataManager,
            LiveLoadOptions opts, CacheManager cacheManager = null) {
            if (_contentDefinition == null)
                throw new InvalidOperationException();
            if (dataManager == null)
                throw new ArgumentNullException(nameof(dataManager));
            var rp = GetResourceProvider();
            dataManager.RegisterDataProvider(rp);
            try {
                return ContentUtil.LoadLiveContent(_contentDefinition, dataManager, opts, cacheManager);
            }
            finally {
                dataManager.DeregisterDataProvider(rp);
            }
        }

        /// <summary>
        /// Get <see cref="IDataProvider"/> to access this container's files
        /// </summary>
        /// <returns><see cref="IDataProvider"/> with this container's files</returns>
        public IDataProvider GetResourceProvider()
            => new CzBoxDataProvider(this);

        /// <summary>
        /// Get enumerator for this container's files
        /// </summary>
        /// <returns>Enumerator for this container's files</returns>
        public IEnumerable<Tuple<string, Stream>> GetDataEnumerator() {
            foreach (var entry in _entries)
                yield return new Tuple<string, Stream>(entry.Key,
                    new CzBoxSubStream(_stream, _bOfs + entry.Value.Item1, entry.Value.Item2));
        }

        /// <summary>
        /// Resource provider based on <see cref="CzBox"/> instance
        /// </summary>
        private class CzBoxDataProvider : IDataProvider {
            private readonly CzBox _box;

            internal CzBoxDataProvider(CzBox box) {
                _box = box;
            }

            public Stream GetStream(Uri uri) {
                if (uri == null)
                    throw new ArgumentNullException(nameof(uri));
                if (!"local".Equals(uri.Host, StringComparison.InvariantCultureIgnoreCase)) return null;
                return _box._entries.TryGetValue(uri.AbsolutePath, out var res)
                    ? new CzBoxSubStream(_box._stream, _box._bOfs + res.Item1, res.Item2)
                    : null;
            }
        }

        /// <summary>
        /// Provides fixed-size read-only access to a <see cref="CzBox"/> container's file
        /// </summary>
        private class CzBoxSubStream : Stream {
            private readonly Stream _stream;
            private readonly long _ofs;
            private readonly int _len;
            private long _cOfs;

            internal CzBoxSubStream(Stream stream, long ofs, int len) {
                _stream = stream ?? throw new ArgumentNullException(nameof(stream));
                _ofs = ofs;
                _len = len;
                _cOfs = _ofs;
            }

            public override void Flush() {
            }

            public override int Read(byte[] buffer, int offset, int count) {
                _stream.Position = _cOfs;
                count = (int) Math.Min(count, _len + _ofs - _cOfs);
                var c = _stream.Read(buffer, offset, count);
                _cOfs += c;
                return c;
            }

            public override long Seek(long offset, SeekOrigin origin) {
                switch (origin) {
                    case SeekOrigin.Begin:
                        return (_cOfs = _stream.Seek(_ofs + offset, origin)) - _ofs;
                    case SeekOrigin.Current:
                        return (_cOfs = _stream.Seek(offset, origin)) - _ofs;
                    case SeekOrigin.End:
                        return (_cOfs = _stream.Seek(_ofs + _len - _stream.Length + offset, origin)) - _ofs;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
                }
            }

            public override void SetLength(long value) => throw new InvalidOperationException();

            public override void Write(byte[] buffer, int offset, int count) => throw new InvalidOperationException();

            public override bool CanRead => _stream.CanRead;
            public override bool CanSeek => _stream.CanSeek;
            public override bool CanWrite => false;
            public override long Length => _len;

            public override long Position {
                get => _cOfs - _ofs;
                set => _cOfs = _ofs + value;
            }
        }
    }
}