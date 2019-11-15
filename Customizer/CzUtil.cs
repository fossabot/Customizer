using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Customizer {
    /// <summary>
    /// General utility functions
    /// </summary>
    internal static class CzUtil {
        internal static long Read7S64(this Stream stream, out int len) {
            ulong value = 0;
            len = 0;
            var bits = 0;
            while (bits < sizeof(long) * 8) {
                var v = stream.ReadByte();
                if (v == -1)
                    throw new EndOfStreamException();
                value |= (ulong) (v & 0x7f) << bits;
                len++;
                bits += 7;
                if (v > 0x7f)
                    break;
            }

            return (long) value;
        }

        internal static int Write7S64(this Stream stream, long value) {
            var uValue = (ulong)value;
            var len = 0;
            do {
                byte v;
                if (uValue < 0x80)
                    v = (byte) (0x80 | (uValue & 0x7f));
                else
                    v = (byte) (uValue & 0x7f);
                stream.WriteByte(v);
                len++;
                uValue >>= 7;
            } while (uValue != 0);

            return len;
        }

        internal static string ReadCString(this Stream stream, int maxLength = int.MaxValue) {
            using (var ms = new MemoryStream()) {
                int c = 0;
                do {
                    var v = stream.ReadByte();
                    if (v == -1 || v == 0)
                        break;
                    ms.WriteByte((byte) v);
                    c++;
                } while (c < maxLength);

                var str = Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int) ms.Length);
                return str;
            }
        }

        internal static short ReadS16(this Stream stream)
            => (short) (stream.ReadByteOrThrow() + (stream.ReadByteOrThrow() << 8));

        internal static ushort ReadU16(this Stream stream)
            => (ushort) ReadS16(stream);

        internal static int ReadS32(this Stream stream)
            => stream.ReadByteOrThrow() + (stream.ReadByteOrThrow() << 8) + (stream.ReadByteOrThrow() << 16) +
               (stream.ReadByteOrThrow() << 24);

        internal static uint ReadU32(this Stream stream)
            => (uint) ReadS32(stream);

        internal static long ReadS64(this Stream stream)
            => stream.ReadByteOrThrow() + ((long) stream.ReadByteOrThrow() << 8) +
               ((long) stream.ReadByteOrThrow() << 16) +
               ((long) stream.ReadByteOrThrow() << 24)
               + ((long) stream.ReadByteOrThrow() << 32) + ((long) stream.ReadByteOrThrow() << 40) +
               ((long) stream.ReadByteOrThrow() << 48) + ((long) stream.ReadByteOrThrow() << 56);

        internal static ulong ReadU64(this Stream stream)
            => (ulong) ReadS64(stream);

        internal static float ReadSingle(this Stream stream) {
            Span<float> span = stackalloc float[1];
            Span<byte> bSpan = MemoryMarshal.Cast<float, byte>(span);
            for (var i = 0; i < sizeof(float); i++)
                bSpan[i] = stream.ReadByteOrThrow();
            return span[0];
        }

        internal static double ReadDouble(this Stream stream) {
            Span<double> span = stackalloc double[1];
            Span<byte> bSpan = MemoryMarshal.Cast<double, byte>(span);
            for (var i = 0; i < sizeof(double); i++)
                bSpan[i] = stream.ReadByteOrThrow();
            return span[0];
        }

        internal static void WriteTo(this short value, Stream stream) {
            stream.WriteByte((byte) value);
            stream.WriteByte((byte) (value >> 8));
        }

        internal static void WriteTo(this ushort value, Stream stream)
            => ((short) value).WriteTo(stream);

        internal static void WriteTo(this int value, Stream stream) {
            stream.WriteByte((byte) value);
            stream.WriteByte((byte) (value >> 8));
            stream.WriteByte((byte) (value >> 16));
            stream.WriteByte((byte) (value >> 24));
        }

        internal static void WriteTo(this uint value, Stream stream)
            => ((int) value).WriteTo(stream);

        internal static void WriteTo(this long value, Stream stream) {
            stream.WriteByte((byte) value);
            stream.WriteByte((byte) (value >> 8));
            stream.WriteByte((byte) (value >> 16));
            stream.WriteByte((byte) (value >> 24));
            stream.WriteByte((byte) (value >> 32));
            stream.WriteByte((byte) (value >> 40));
            stream.WriteByte((byte) (value >> 48));
            stream.WriteByte((byte) (value >> 56));
        }

        internal static void WriteTo(this ulong value, Stream stream)
            => ((long) value).WriteTo(stream);

        internal static void WriteTo(this float value, Stream stream) {
            Span<byte> span = stackalloc byte[sizeof(float)];
            MemoryMarshal.Cast<byte, float>(span)[0] = value;
            for (var i = 0; i < sizeof(float); i++)
                stream.WriteByte(span[i]);
        }

        internal static void WriteTo(this double value, Stream stream) {
            Span<byte> span = stackalloc byte[sizeof(double)];
            MemoryMarshal.Cast<byte, double>(span)[0] = value;
            for (var i = 0; i < sizeof(double); i++)
                stream.WriteByte(span[i]);
        }

        internal static byte ReadByteOrThrow(this Stream stream) {
            var res = stream.ReadByte();
            if (res == -1)
                throw new EndOfStreamException();
            return (byte) res;
        }
    }
}