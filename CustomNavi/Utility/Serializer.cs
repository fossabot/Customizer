using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
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
                var arr = new UTF8Encoding(false).GetBytes(vString);
                stream.Write(arr, 0, arr.Length);
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
            if (t.GetCustomAttribute<CnCustomSerializeMembersAttribute>() != null) {
                foreach (var m in members) {
                    if (m.GetCustomAttribute<CnSerializeAttribute>() is CnSerializeAttribute attrib)
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
                if (t == typeof(sbyte)) {
                    for (var i = 0; i < count; i++)
                        vArray.SetValue(stream.ReadByteOrThrow(), i);
                }

                else if (t == typeof(byte)) {
                    for (var i = 0; i < count; i++)
                        vArray.SetValue(stream.ReadByteOrThrow(), i);
                }

                else if (t == typeof(short)) {
                    for (var i = 0; i < count; i++)
                        vArray.SetValue(stream.ReadS16(), i);
                }

                else if (t == typeof(ushort)) {
                    for (var i = 0; i < count; i++)
                        vArray.SetValue(stream.ReadS32(), i);
                }

                else if (t == typeof(int)) {
                    for (var i = 0; i < count; i++)
                        vArray.SetValue(stream.ReadS32(), i);
                }

                else if (t == typeof(uint)) {
                    for (var i = 0; i < count; i++)
                        vArray.SetValue(stream.ReadU32(), i);
                }

                else if (t == typeof(long)) {
                    for (var i = 0; i < count; i++)
                        vArray.SetValue(stream.ReadS64(), i);
                }

                else if (t == typeof(ulong)) {
                    for (var i = 0; i < count; i++)
                        vArray.SetValue(stream.ReadU64(), i);
                }

                else if (t == typeof(float)) {
                    for (var i = 0; i < count; i++)
                        vArray.SetValue(stream.ReadSingle(), i);
                }

                else if (t == typeof(double)) {
                    for (var i = 0; i < count; i++)
                        vArray.SetValue(stream.ReadDouble(), i);
                }
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
                var targs = t.GetGenericArguments();
                var t2 = targs[0];
                var t3 = targs[1];
                for (var i = 0; i < count; i++)
                    vDict.Add(Deserialize(t2, stream), Deserialize(t3, stream));

                return vDict;
            }

            var members = t.GetMembers();
            if (t.GetCustomAttribute<CnCustomSerializeMembersAttribute>() != null) {
                var dict = new Dictionary<int, MemberInfo>(members.Length);
                foreach (var m in members) {
                    if (m.GetCustomAttribute<CnSerializeAttribute>() is CnSerializeAttribute attrib)
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
                    var fres = Deserialize(fi.FieldType, stream);
                    ((FieldInfo) info).SetValue(target, fres);
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