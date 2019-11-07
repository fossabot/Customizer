using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace CustomNavi.Utility {
    /// <summary>
    /// Basic serialization class 
    /// </summary>
    public static class Serializer {
        /// <summary>
        /// Serialize object to stream
        /// </summary>
        /// <param name="stream">Stream to write to</param>
        /// <param name="value">Object to serialize</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is null</exception>
        public static void Serialize(Stream stream, object value) {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (value == null) {
                stream.WriteByte(0);
                return;
            }

            switch (value) {
                case sbyte vSByte:
                    stream.WriteByte((byte) vSByte);
                    return;
                case byte vByte:
                    stream.WriteByte(vByte);
                    return;
                case short vShort:
                    vShort.WriteTo(stream);
                    return;
                case ushort vUShort:
                    vUShort.WriteTo(stream);
                    return;
                case int vInt:
                    vInt.WriteTo(stream);
                    return;
                case uint vUInt:
                    vUInt.WriteTo(stream);
                    return;
                case long vLong:
                    vLong.WriteTo(stream);
                    return;
                case ulong vULong:
                    vULong.WriteTo(stream);
                    return;
                case float vFloat:
                    vFloat.WriteTo(stream);
                    return;
                case double vDouble:
                    vDouble.WriteTo(stream);
                    return;
            }

            var t = value.GetType();

            if (!t.IsValueType)
                stream.WriteByte(1);

            if (t.IsEnum) {
                // ReSharper disable once TailRecursiveCall
                Serialize(stream, Convert.ChangeType(value, t.GetEnumUnderlyingType(), null));
                return;
            }

            if (value is string vString) {
                using (var w = new StreamWriter(stream, Encoding.UTF8, 4096, true))
                    w.Write(vString);
                stream.WriteByte(0);
                return;
            }

            if (value is Array vArray) {
                vArray.Length.WriteTo(stream);
                foreach (var o in vArray)
                    Serialize(stream, o);
                return;
            }

            if (IsList(value)) {
                var vList = value as IList;
                Debug.Assert(vList != null, nameof(vList) + " != null");
                vList.Count.WriteTo(stream);
                foreach (var o in vList)
                    Serialize(stream, o);
                return;
            }

            if (IsDictionary(value)) {
                var vDict = value as IDictionary;
                Debug.Assert(vDict != null, nameof(vDict) + " != null");
                vDict.Count.WriteTo(stream);
                foreach (var o in vDict.Keys) {
                    Serialize(stream, o);
                    Serialize(stream, vDict[o]);
                }

                return;
            }

            var members = t.GetMembers();
            if (t.GetCustomAttribute<NCustomSerializeMembersAttribute>() != null) {
                foreach (var m in members) {
                    if (m.GetCustomAttribute<NSerializeAttribute>() is NSerializeAttribute attrib)
                        TrySerializeMember(value, m, stream, attrib.Tag);
                }

                (-1).WriteTo(stream);
            }
            else
                foreach (var m in members)
                    TrySerializeMember(value, m, stream);
        }

        private static void TrySerializeMember(object src, MemberInfo info, Stream stream, int? tag = null) {
            switch (info.MemberType) {
                case MemberTypes.Field:
                    tag?.WriteTo(stream);
                    Serialize(stream, ((FieldInfo) info).GetValue(src));
                    break;
                case MemberTypes.Property:
                    tag?.WriteTo(stream);
                    var pi = (PropertyInfo) info;
                    if (pi.CanRead && pi.CanWrite)
                        Serialize(stream, pi.GetValue(src));
                    break;
            }
        }

        /// <summary>
        /// Deserialize object of given type
        /// </summary>
        /// <param name="span">Span to read from</param>
        /// <param name="offset">Offset in span to start at</param>
        /// <returns>Deserialized object</returns>
        public static T Deserialize<T>(Span<byte> span, int offset = 0) {
            Deserialize(typeof(T), span, out var ires, offset);
            return (T) ires;
        }

        /// <summary>
        /// Deserialize object of given type
        /// </summary>
        /// <param name="span">Span to read from</param>
        /// <param name="res">Deserialized object</param>
        /// <param name="offset">Offset in span to start at</param>
        /// <returns>Length of read serialized data</returns>
        public static int Deserialize<T>(Span<byte> span, out T res, int offset = 0) {
            var len = Deserialize(typeof(T), span, out var ires, offset);
            res = (T) ires;
            return len;
        }

        /// <summary>
        /// Deserialize object of given type
        /// </summary>
        /// <param name="t">Target deserialized type</param>
        /// <param name="span">Span to read from</param>
        /// <param name="res">Deserialized object</param>
        /// <param name="offset">Offset in span to start at</param>
        /// <returns>Length of read serialized data</returns>
        /// <exception cref="ArgumentNullException"><paramref name="t"/> is null</exception>
        public static int Deserialize(Type t, Span<byte> span, out object res, int offset = 0) {
            if (t == null)
                throw new ArgumentNullException(nameof(t));
            if (t == typeof(sbyte)) {
                res = (sbyte) span[offset];
                return 1;
            }

            if (t == typeof(byte)) {
                res = span[offset];
                return 1;
            }

            if (t == typeof(short)) {
                res = ReadS16(span, offset);
                return 2;
            }

            if (t == typeof(ushort)) {
                res = ReadU16(span, offset);
                return 2;
            }

            if (t == typeof(int)) {
                res = ReadS32(span, offset);
                return 4;
            }

            if (t == typeof(uint)) {
                res = ReadU32(span, offset);
                return 4;
            }

            if (t == typeof(long)) {
                res = ReadS64(span, offset);
                return 8;
            }

            if (t == typeof(ulong)) {
                res = ReadU64(span, offset);
                return 8;
            }

            if (t == typeof(float)) {
                res = ReadSingle(span, offset);
                return 4;
            }

            if (t == typeof(double)) {
                res = ReadDouble(span, offset);
                return 8;
            }

            var len = t.IsValueType ? 0 : 1;

            if (!t.IsValueType && span[0] == 0) {
                res = null;
                return len;
            }

            if (t.IsEnum) {
                var et = t.GetEnumUnderlyingType();
                return Deserialize(et, span, out res, offset);
            }

            if (t == typeof(string)) {
                len += ReadCString(span.Slice(offset + len), out var sres);
                res = sres;
                return len;
            }

            if (t.IsArray) {
                var count = ReadS32(span, offset + len);
                len += 4;
                var t2 = t.GetElementType() ??
                         throw new Exception($"Element type for array not found in object of type {t}");
                var vArray = Array.CreateInstance(t2, count);
                res = vArray;
                // Possible shortcut deserialization for primitives
                if (t == typeof(sbyte)) {
                    for (var i = 0; i < count; i++) {
                        vArray.SetValue(span[offset + len], i);
                        len++;
                    }
                }

                else if (t == typeof(byte)) {
                    for (var i = 0; i < count; i++) {
                        vArray.SetValue(span[offset + len], i);
                        len++;
                    }
                }

                else if (t == typeof(short)) {
                    for (var i = 0; i < count; i++) {
                        vArray.SetValue(ReadS16(span, offset + len), i);
                        len += 2;
                    }
                }

                else if (t == typeof(ushort)) {
                    for (var i = 0; i < count; i++) {
                        vArray.SetValue(ReadU16(span, offset + len), i);
                        len += 2;
                    }
                }

                else if (t == typeof(int)) {
                    for (var i = 0; i < count; i++) {
                        vArray.SetValue(ReadS32(span, offset + len), i);
                        len += 4;
                    }
                }

                else if (t == typeof(uint)) {
                    for (var i = 0; i < count; i++) {
                        vArray.SetValue(ReadU32(span, offset + len), i);
                        len += 4;
                    }
                }

                else if (t == typeof(long)) {
                    for (var i = 0; i < count; i++) {
                        vArray.SetValue(ReadS64(span, offset + len), i);
                        len += 8;
                    }
                }

                else if (t == typeof(ulong)) {
                    for (var i = 0; i < count; i++) {
                        vArray.SetValue(ReadU64(span, offset + len), i);
                        len += 8;
                    }
                }

                else if (t == typeof(float)) {
                    for (var i = 0; i < count; i++) {
                        vArray.SetValue(ReadSingle(span, offset + len), i);
                        len += 4;
                    }
                }

                else if (t == typeof(double)) {
                    for (var i = 0; i < count; i++) {
                        vArray.SetValue(ReadDouble(span, offset + len), i);
                        len += 8;
                    }
                }
                else
                    for (var i = 0; i < count; i++) {
                        len += Deserialize(t2, span, out var res2, offset + len);
                        vArray.SetValue(res2, i);
                    }

                return len;
            }

            res = Activator.CreateInstance(t);

            if (IsListType(t)) {
                var vList = res as IList;
                Debug.Assert(vList != null, nameof(vList) + " != null");
                var count = ReadS32(span, offset + len);
                len += 4;
                var t2 = t.GetGenericArguments()[0];
                for (var i = 0; i < count; i++) {
                    len += Deserialize(t2, span, out var res2, offset + len);
                    vList.Add(res2);
                }

                return len;
            }

            if (IsDictionaryType(t)) {
                var vDict = res as IDictionary;
                Debug.Assert(vDict != null, nameof(vDict) + " != null");
                var count = ReadS32(span, offset + len);
                len += 4;
                var targs = t.GetGenericArguments();
                var t2 = targs[0];
                var t3 = targs[1];
                for (var i = 0; i < count; i++) {
                    len += Deserialize(t2, span, out var res2, offset + len);
                    len += Deserialize(t3, span, out var res3, offset + len);
                    vDict.Add(res2, res3);
                }

                return len;
            }

            var members = t.GetMembers();
            if (t.GetCustomAttribute<NCustomSerializeMembersAttribute>() != null) {
                var dict = new Dictionary<int, MemberInfo>(members.Length);
                foreach (var m in members) {
                    if (m.GetCustomAttribute<NSerializeAttribute>() is NSerializeAttribute attrib)
                        dict.Add(attrib.Tag, m);
                }

                int tag;
                do {
                    tag = ReadS32(span, offset + len);
                    len += 4;
                    if (tag != -1)
                        len += TryDeserializeMember(dict[tag], span, res, offset + len);
                } while (tag != -1);
            }
            else
                foreach (var m in members)
                    len += TryDeserializeMember(m, span, res, offset + len);

            return len;
        }

        private static int TryDeserializeMember(MemberInfo info, Span<byte> span, object target, int offset) {
            int len;
            switch (info.MemberType) {
                case MemberTypes.Field:
                    var fi = (FieldInfo) info;
                    len = Deserialize(fi.FieldType, span, out var fres, offset);
                    ((FieldInfo) info).SetValue(target, fres);
                    return len;
                case MemberTypes.Property:
                    var pi = (PropertyInfo) info;
                    if (pi.CanRead && pi.CanWrite) {
                        len = Deserialize(pi.PropertyType, span, out var pres, offset);
                        ((PropertyInfo) info).SetValue(target, pres);
                        return len;
                    }

                    break;
            }

            return 0;
        }

        private static int ReadCString(Span<byte> span, out string res, int offset = 0, int maxLength = int.MaxValue) {
            var sb = new StringBuilder();
            int v, c = 0;
            do {
                v = span[offset + c];
                if (v != 0)
                    sb.Append((char) v);
                c++;
            } while (v != 0 && c < maxLength);

            res = sb.ToString();
            return c;
        }

        private static short ReadS16(Span<byte> span, int offset = 0)
            => (short) (span[offset] + (span[offset + 1] << 8));

        private static ushort ReadU16(Span<byte> span, int offset = 0)
            => (ushort) ReadS16(span, offset);

        private static int ReadS32(Span<byte> span, int offset = 0)
            => span[offset] + (span[offset + 1] << 8) + (span[offset + 2] << 16) +
               (span[offset + 3] << 24);

        private static uint ReadU32(Span<byte> span, int offset = 0)
            => (uint) ReadS32(span, offset);

        private static long ReadS64(Span<byte> span, int offset = 0)
            => span[offset] + ((long) span[offset + 1] << 8) + ((long) span[offset + 2] << 16) +
               ((long) span[offset + 3] << 24)
               + ((long) span[offset + 4] << 32) + ((long) span[offset + 5] << 40) +
               ((long) span[offset + 6] << 48) + ((long) span[offset + 7] << 56);

        private static ulong ReadU64(Span<byte> span, int offset = 0)
            => (ulong) ReadS64(span, offset);

        private static float ReadSingle(Span<byte> span, int offset = 0)
            => MemoryMarshal.Cast<byte, float>(span.Slice(offset, 4))[0];

        private static double ReadDouble(Span<byte> span, int offset = 0)
            => MemoryMarshal.Cast<byte, double>(span.Slice(offset, 8))[0];

        private static void WriteTo(this short value, Stream stream) {
            stream.WriteByte((byte) value);
            stream.WriteByte((byte) (value >> 8));
        }

        private static void WriteTo(this ushort value, Stream stream)
            => ((short) value).WriteTo(stream);

        private static void WriteTo(this int value, Stream stream) {
            stream.WriteByte((byte) value);
            stream.WriteByte((byte) (value >> 8));
            stream.WriteByte((byte) (value >> 16));
            stream.WriteByte((byte) (value >> 24));
        }

        private static void WriteTo(this uint value, Stream stream)
            => ((int) value).WriteTo(stream);

        private static void WriteTo(this long value, Stream stream) {
            stream.WriteByte((byte) value);
            stream.WriteByte((byte) (value >> 8));
            stream.WriteByte((byte) (value >> 16));
            stream.WriteByte((byte) (value >> 24));
            stream.WriteByte((byte) (value >> 32));
            stream.WriteByte((byte) (value >> 40));
            stream.WriteByte((byte) (value >> 48));
            stream.WriteByte((byte) (value >> 56));
        }

        private static void WriteTo(this ulong value, Stream stream)
            => ((long) value).WriteTo(stream);

        private static void WriteTo(this float value, Stream stream) {
            Span<byte> span = stackalloc byte[sizeof(float)];
            MemoryMarshal.Cast<byte, float>(span)[0] = value;
            for (var i = 0; i < sizeof(float); i++)
                stream.WriteByte(span[i]);
        }

        private static void WriteTo(this double value, Stream stream) {
            Span<byte> span = stackalloc byte[sizeof(double)];
            MemoryMarshal.Cast<byte, double>(span)[0] = value;
            for (var i = 0; i < sizeof(double); i++)
                stream.WriteByte(span[i]);
        }

        private static bool IsList(object o) {
            if (o == null) return false;
            return o is IList && IsListType(o.GetType());
        }

        private static bool IsListType(Type t)
            => t.IsGenericType && t.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));

        private static bool IsDictionary(object o) {
            if (o == null) return false;
            return o is IDictionary && IsDictionaryType(o.GetType());
        }

        private static bool IsDictionaryType(Type t)
            => t.IsGenericType &&
               t.GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>));
    }
}