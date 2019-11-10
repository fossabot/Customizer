using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using CustomNavi.Content;
using CustomNavi.Utility;

namespace CustomNavi.Box {
    public class CnBox {
        private Stream _stream;
        private ContentDefinition _contentDefinition;
        private Dictionary<string, Tuple<int, int>> _entries;
        private long _bOfs;

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

        public static CnBox Load(Stream stream) {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            var baseOff = stream.Read7B(out var sbLen);
            var res = new CnBox {
                _stream = stream, _entries = new Dictionary<string, Tuple<int, int>>(),
                _bOfs = baseOff + sbLen
            };
            using (var ds = new DeflateStream(stream, CompressionMode.Decompress, true)) {
                res._contentDefinition = Serializer.Deserialize<ContentDefinition>(ds);
                var ofs = 0;
                var list = Serializer.Deserialize<List<MutableKeyValuePair<string, int>>>(ds);
                if (list != null)
                    foreach (var entry in list) {
                        res._entries.Add(entry.Item1, new Tuple<int, int>(ofs, entry.Item2));
                        ofs += entry.Item2;
                    }
            }

            return res;
        }

        public ContentDefinition GetContentDefinition()
            => (ContentDefinition) _contentDefinition.Clone();

        public IResourceProvider GetResourceProvider()
            => new CnBoxResourceProvider(this);

        public IEnumerable<Tuple<string, Stream>> GetResourceEnumerator() {
            foreach (var entry in _entries)
                yield return new Tuple<string, Stream>(entry.Key,
                    new CnBoxSubStream(_stream, _bOfs + entry.Value.Item1, entry.Value.Item2));
        }

        private class CnBoxResourceProvider : IResourceProvider {
            private readonly CnBox _box;

            internal CnBoxResourceProvider(CnBox box) {
                _box = box;
            }

            public Stream GetStream(Uri uri) {
                if (uri == null)
                    throw new ArgumentNullException(nameof(uri));
                if (!"cnbox".Equals(uri.Host, StringComparison.InvariantCultureIgnoreCase)) return null;
                return _box._entries.TryGetValue(uri.AbsolutePath, out var res)
                    ? new CnBoxSubStream(_box._stream, _box._bOfs + res.Item1, res.Item2)
                    : null;
            }
        }

        private class CnBoxSubStream : Stream {
            private readonly Stream _stream;
            private readonly long _ofs;
            private readonly int _len;
            private long _cOfs;

            internal CnBoxSubStream(Stream stream, long ofs, int len) {
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