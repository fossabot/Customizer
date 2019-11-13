using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Customizer.Utility {
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
                var arr = new UTF8Encoding(false).GetBytes(vString);
                stream.Write(arr, 0, arr.Length);
                stream.WriteByte(0);
                return;
            }

            if (value is Array vArray) {
                vArray.Length.WriteTo(stream);
                if (vArray is sbyte[] vArrayS8)
                    stream.Write(MemoryMarshal.Cast<sbyte, byte>(vArrayS8));
                else if (vArray is byte[] vArrayU8)
                    stream.Write(vArrayU8, 0, vArray.Length);
                else if (vArray is short[] vArrayS16)
                    if (BitConverter.IsLittleEndian)
                        stream.Write(MemoryMarshal.Cast<short, byte>(vArrayS16));
                    else
                        foreach (var o in vArrayS16)
                            o.WriteTo(stream);
                else if (vArray is ushort[] vArrayU16)
                    if (BitConverter.IsLittleEndian)
                        stream.Write(MemoryMarshal.Cast<ushort, byte>(vArrayU16));
                    else
                        foreach (var o in vArrayU16)
                            o.WriteTo(stream);
                else if (vArray is int[] vArrayS32)
                    if (BitConverter.IsLittleEndian)
                        stream.Write(MemoryMarshal.Cast<int, byte>(vArrayS32));
                    else
                        foreach (var o in vArrayS32)
                            o.WriteTo(stream);
                else if (vArray is uint[] vArrayU32)
                    if (BitConverter.IsLittleEndian)
                        stream.Write(MemoryMarshal.Cast<uint, byte>(vArrayU32));
                    else
                        foreach (var o in vArrayU32)
                            o.WriteTo(stream);
                else if (vArray is long[] vArrayS64)
                    if (BitConverter.IsLittleEndian)
                        stream.Write(MemoryMarshal.Cast<long, byte>(vArrayS64));
                    else
                        foreach (var o in vArrayS64)
                            o.WriteTo(stream);
                else if (vArray is ulong[] vArrayU64)
                    if (BitConverter.IsLittleEndian)
                        stream.Write(MemoryMarshal.Cast<ulong, byte>(vArrayU64));
                    else
                        foreach (var o in vArrayU64)
                            o.WriteTo(stream);
                else if (vArray is float[] vArraySingle)
                    stream.Write(MemoryMarshal.Cast<float, byte>(vArraySingle));
                else if (vArray is double[] vArrayDouble)
                    stream.Write(MemoryMarshal.Cast<double, byte>(vArrayDouble));
                else
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
            if (t.GetCustomAttribute<CzCustomSerializeMembersAttribute>() != null) {
                foreach (var m in members) {
                    if (m.GetCustomAttribute<CzSerializeAttribute>() is CzSerializeAttribute attrib)
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
        /// <param name="stream">Stream to read from</param>
        /// <returns>Deserialized object</returns>
        public static T Deserialize<T>(Stream stream)
            => (T) Deserialize(typeof(T), stream);

        /// <summary>
        /// Deserialize object of given type
        /// </summary>
        /// <param name="t">Target deserialized type</param>
        /// <param name="stream">Stream to read from</param>
        /// <returns>Deserialized object</returns>
        /// <exception cref="ArgumentNullException"><paramref name="t"/> or <paramref name="stream"/> are null</exception>
        public static object Deserialize(Type t, Stream stream) {
            if (t == null)
                throw new ArgumentNullException(nameof(t));
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (t == typeof(sbyte))
                return (sbyte) stream.ReadByteOrThrow();
            if (t == typeof(byte))
                return stream.ReadByteOrThrow();
            if (t == typeof(short))
                return stream.ReadS16();
            if (t == typeof(ushort))
                return stream.ReadU16();
            if (t == typeof(int))
                return stream.ReadS32();
            if (t == typeof(uint))
                return stream.ReadU32();
            if (t == typeof(long))
                return stream.ReadS64();
            if (t == typeof(ulong))
                return stream.ReadU64();
            if (t == typeof(float))
                return stream.ReadSingle();
            if (t == typeof(double))
                return stream.ReadDouble();
            if (!t.IsValueType && stream.ReadByteOrThrow() == 0)
                return null;
            if (t.IsEnum)
                return Deserialize(t.GetEnumUnderlyingType(), stream);
            if (t == typeof(string))
                return stream.ReadCString();

            if (t.IsArray) {
                var count = stream.ReadS32();
                var t2 = t.GetElementType() ??
                         throw new Exception($"Element type for array not found in object of type {t}");
                var vArray = Array.CreateInstance(t2, count);
                if (vArray is sbyte[] vArrayS8)
                    stream.Read(MemoryMarshal.Cast<sbyte, byte>(vArrayS8));
                else if (vArray is byte[] vArrayU8)
                    stream.Read(vArrayU8);
                else if (vArray is short[] vArrayS16)
                    if (BitConverter.IsLittleEndian)
                        stream.Read(MemoryMarshal.Cast<short, byte>(vArrayS16));
                    else
                        for (var i = 0; i < count; i++)
                            vArrayS16.SetValue(stream.ReadS16(), i);
                else if (vArray is ushort[] vArrayU16)
                    if (BitConverter.IsLittleEndian)
                        stream.Read(MemoryMarshal.Cast<ushort, byte>(vArrayU16));
                    else
                        for (var i = 0; i < count; i++)
                            vArrayU16.SetValue(stream.ReadU16(), i);
                else if (vArray is int[] vArrayS32)
                    if (BitConverter.IsLittleEndian)
                        stream.Read(MemoryMarshal.Cast<int, byte>(vArrayS32));
                    else
                        for (var i = 0; i < count; i++)
                            vArrayS32.SetValue(stream.ReadS32(), i);
                else if (vArray is uint[] vArrayU32)
                    if (BitConverter.IsLittleEndian)
                        stream.Read(MemoryMarshal.Cast<uint, byte>(vArrayU32));
                    else
                        for (var i = 0; i < count; i++)
                            vArrayU32.SetValue(stream.ReadU32(), i);
                else if (vArray is long[] vArrayS64)
                    if (BitConverter.IsLittleEndian)
                        stream.Read(MemoryMarshal.Cast<long, byte>(vArrayS64));
                    else
                        for (var i = 0; i < count; i++)
                            vArrayS64.SetValue(stream.ReadS64(), i);
                else if (vArray is ulong[] vArrayU64)
                    if (BitConverter.IsLittleEndian)
                        stream.Read(MemoryMarshal.Cast<ulong, byte>(vArrayU64));
                    else
                        for (var i = 0; i < count; i++)
                            vArrayU64.SetValue(stream.ReadU64(), i);
                else if (vArray is float[] vArraySingle)
                    stream.Read(MemoryMarshal.Cast<float, byte>(vArraySingle));
                else if (vArray is double[] vArrayDouble)
                    stream.Read(MemoryMarshal.Cast<double, byte>(vArrayDouble));
                else
                    for (var i = 0; i < count; i++)
                        vArray.SetValue(Deserialize(t2, stream), i);

                return vArray;
            }

            var res = Activator.CreateInstance(t);

            if (IsListType(t)) {
                var vList = res as IList;
                Debug.Assert(vList != null, nameof(vList) + " != null");
                var count = stream.ReadS32();
                var t2 = t.GetGenericArguments()[0];
                for (var i = 0; i < count; i++)
                    vList.Add(Deserialize(t2, stream));

                return vList;
            }

            if (IsDictionaryType(t)) {
                var vDict = res as IDictionary;
                Debug.Assert(vDict != null, nameof(vDict) + " != null");
                var count = stream.ReadS32();
                var typeArgs = t.GetGenericArguments();
                var t2 = typeArgs[0];
                var t3 = typeArgs[1];
                for (var i = 0; i < count; i++)
                    vDict.Add(Deserialize(t2, stream), Deserialize(t3, stream));

                return vDict;
            }

            var members = t.GetMembers();
            if (t.GetCustomAttribute<CzCustomSerializeMembersAttribute>() != null) {
                var dict = new Dictionary<int, MemberInfo>(members.Length);
                foreach (var m in members) {
                    if (m.GetCustomAttribute<CzSerializeAttribute>() is CzSerializeAttribute attrib)
                        dict.Add(attrib.Tag, m);
                }

                int tag;
                do {
                    tag = stream.ReadS32();
                    if (tag != -1)
                        TryDeserializeMember(dict[tag], stream, res);
                } while (tag != -1);
            }
            else
                foreach (var m in members)
                    TryDeserializeMember(m, stream, res);

            return res;
        }

        private static void TryDeserializeMember(MemberInfo info, Stream stream, object target) {
            switch (info.MemberType) {
                case MemberTypes.Field:
                    var fi = (FieldInfo) info;
                    var fieldRes = Deserialize(fi.FieldType, stream);
                    ((FieldInfo) info).SetValue(target, fieldRes);
                    break;
                case MemberTypes.Property:
                    var pi = (PropertyInfo) info;
                    if (pi.CanRead && pi.CanWrite) {
                        var pres = Deserialize(pi.PropertyType, stream);
                        ((PropertyInfo) info).SetValue(target, pres);
                    }

                    break;
            }
        }

        private static void Write(this Stream stream, Span<byte> span) {
            var arr = ArrayPool<byte>.Shared.Rent(4096);
            var arrLen = arr.Length;
            try {
                var read = 0;
                var left = span.Length;
                while (left > 0) {
                    var len = Math.Min(left, arrLen);
                    span.Slice(read, len).CopyTo(arr);
                    stream.Write(arr, 0, len);
                    read += len;
                    left -= len;
                }
            }
            finally {
                ArrayPool<byte>.Shared.Return(arr);
            }
        }

        private static void Read(this Stream stream, Span<byte> span) {
            var arr = ArrayPool<byte>.Shared.Rent(4096);
            var arrLen = arr.Length;
            try {
                var read = 0;
                var left = span.Length;
                while (left > 0) {
                    var len = Math.Min(left, arrLen);
                    var aLen = stream.Read(arr, 0, len);
                    arr.AsSpan(0, aLen).CopyTo(span.Slice(read, aLen));
                    read += aLen;
                    left -= aLen;
                }
            }
            finally {
                ArrayPool<byte>.Shared.Return(arr);
            }
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