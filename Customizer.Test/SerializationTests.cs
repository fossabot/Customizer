using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Customizer.Utility;
using NUnit.Framework;

namespace Customizer.Test {
    public class SerializationTests {
        [SetUp]
        public void Setup() {
        }

        [Test]
        public void TestSerializeText() {
            const string str = "VS.ネビュラグレイ";
            var ms = new MemoryStream();
            Serializer.Serialize(ms, str);
            ms.Position = 0;
            var str2 = Serializer.Deserialize<string>(ms);
            Assert.AreEqual(str, str2);
        }

        [Test]
        public void TestSerializeNormal() {
            var ms = new MemoryStream();
            var y = new A {
                w = 100,
                H = 200,
                a = new sbyte[] {0, 1, 2, 3},
                l = new List<string> {"testerin", "bonit"},
                d = new Dictionary<string, string> {{"hi", "babe"}, {"we", "rock"}},
                e = E.B,
                v = new Vector3(1.0f, 2.0f, 3.0f),
                a2 = new[] {0x1000, 0x1033140, 0x10f0833983}
            };
            Serializer.Serialize(ms, y);
            ms.Position = 0;
            var z = Serializer.Deserialize<A>(ms);
            Assert.AreEqual(y.w, z.w, "field check fail");
            Assert.AreEqual(y.H, z.H, "property check fail");
            Assert.IsNotNull(z.a, "array null check fail");
            Assert.AreEqual(y.a.Length, z.a.Length, "array length check fail");
            for (var i = 0; i < y.a.Length; i++)
                Assert.AreEqual(y.a[i], z.a[i], $"array entry {i + 1} check fail");
            Assert.IsNotNull(z.a2, "array2 null check fail");
            Assert.AreEqual(y.a2.Length, z.a2.Length, "array2 length check fail");
            for (var i = 0; i < y.a2.Length; i++)
                Assert.AreEqual(y.a2[i], z.a2[i], $"array2 entry {i + 1} check fail");
            Assert.IsNotNull(z.l, "list null check fail");
            Assert.AreEqual(y.l.Count, z.l.Count, "list length check fail");
            Assert.AreEqual(y.l[0], z.l[0], "list entry 1 check fail");
            Assert.AreEqual(y.l[1], z.l[1], "list entry 2 check fail");
            Assert.IsNotNull(z.d, "dict null check fail");
            Assert.AreEqual(y.d.Count, z.d.Count, "dict length check fail");
            foreach (var e in y.d.Keys)
                Assert.AreEqual(y.d[e], z.d[e], "dict entry check fail");
            Assert.AreEqual(y.e, z.e, "enum check fail");
            Assert.AreEqual(y.v.X, z.v.X, "struct x check fail");
            Assert.AreEqual(y.v.Y, z.v.Y, "struct y check fail");
            Assert.AreEqual(y.v.Z, z.v.Z, "struct z check fail");
        }

        [Test]
        public void TestSerializeCustom() {
            var ms = new MemoryStream();
            var y = new X {
                w = 100,
                H = 200,
                a = new byte[] {0, 1, 2, 3},
                l = new List<string> {"testerin", "bonit"},
                d = new Dictionary<string, string> {{"hi", "babe"}, {"we", "rock"}},
                e = E.B,
                v = new Vector3(1.0f, 2.0f, 3.0f),
                a2 = new[] {0x1000, 0x1033140, 0x10f0833983}
            };
            Serializer.Serialize(ms, y);
            ms.Position = 0;
            var z = Serializer.Deserialize<X>(ms);
            Assert.AreEqual(y.w, z.w, "field check fail");
            Assert.AreEqual(y.H, z.H, "property check fail");
            Assert.IsNotNull(z.a, "array null check fail");
            Assert.AreEqual(y.a.Length, z.a.Length, "array length check fail");
            for (var i = 0; i < y.a.Length; i++)
                Assert.AreEqual(y.a[i], z.a[i], $"array entry {i + 1} check fail");
            Assert.IsNotNull(z.a2, "array2 null check fail");
            Assert.AreEqual(y.a2.Length, z.a2.Length, "array2 length check fail");
            for (var i = 0; i < y.a2.Length; i++)
                Assert.AreEqual(y.a2[i], z.a2[i], $"array2 entry {i + 1} check fail");
            Assert.IsNotNull(z.l, "list null check fail");
            Assert.AreEqual(y.l.Count, z.l.Count, "list length check fail");
            Assert.AreEqual(y.l[0], z.l[0], "list entry 1 check fail");
            Assert.AreEqual(y.l[1], z.l[1], "list entry 2 check fail");
            Assert.IsNotNull(z.d, "dict null check fail");
            Assert.AreEqual(y.d.Count, z.d.Count, "dict length check fail");
            foreach (var e in y.d.Keys)
                Assert.AreEqual(y.d[e], z.d[e], "dict entry check fail");
            Assert.AreEqual(y.e, z.e, "enum check fail");
            Assert.AreEqual(y.v.X, z.v.X, "struct x check fail");
            Assert.AreEqual(y.v.Y, z.v.Y, "struct y check fail");
            Assert.AreEqual(y.v.Z, z.v.Z, "struct z check fail");
        }

        [Test]
        public void TestSerializeCustomExtend() {
            var ms = new MemoryStream();
            var y = new X {
                w = 100,
                H = 200,
                a = new byte[] {0, 1, 2, 3},
                l = new List<string> {"testerin", "bonit"},
                d = new Dictionary<string, string> {{"hi", "babe"}, {"we", "rock"}},
                e = E.B,
                v = new Vector3(1.0f, 2.0f, 3.0f),
                a2 = new[] {0x1000, 0x1033140, 0x10f0833983}
            };
            Serializer.Serialize(ms, y);
            ms.Position = 0;
            var z = Serializer.Deserialize<Xe>(ms);
            Assert.AreEqual(y.w, z.w, "field check fail");
            Assert.AreEqual(y.H, z.H, "property check fail");
            Assert.IsNotNull(z.a, "array null check fail");
            Assert.AreEqual(y.a.Length, z.a.Length, "array length check fail");
            for (var i = 0; i < y.a.Length; i++)
                Assert.AreEqual(y.a[i], z.a[i], $"array entry {i + 1} check fail");
            Assert.IsNotNull(z.a2, "array2 null check fail");
            Assert.AreEqual(y.a2.Length, z.a2.Length, "array2 length check fail");
            for (var i = 0; i < y.a2.Length; i++)
                Assert.AreEqual(y.a2[i], z.a2[i], $"array2 entry {i + 1} check fail");
            Assert.IsNotNull(z.l, "list null check fail");
            Assert.AreEqual(y.l.Count, z.l.Count, "list length check fail");
            Assert.AreEqual(y.l[0], z.l[0], "list entry 1 check fail");
            Assert.AreEqual(y.l[1], z.l[1], "list entry 2 check fail");
            Assert.IsNotNull(z.d, "dict null check fail");
            Assert.AreEqual(y.d.Count, z.d.Count, "dict length check fail");
            foreach (var e in y.d.Keys)
                Assert.AreEqual(y.d[e], z.d[e], "dict entry check fail");
            Assert.AreEqual(y.e, z.e, "enum check fail");
            Assert.AreEqual(y.v.X, z.v.X, "struct x check fail");
            Assert.AreEqual(y.v.Y, z.v.Y, "struct y check fail");
            Assert.AreEqual(y.v.Z, z.v.Z, "struct z check fail");
        }

        [CzCustomSerializeMembers]
        private class X {
            [CzSerialize(0)] public int w;
            [CzSerialize(1)] public int H { get; set; }

            [CzSerialize(2)] public byte[] a;
            [CzSerialize(3)] public List<string> l;
            [CzSerialize(4)] public Dictionary<string, string> d;
            [CzSerialize(5)] public E e;
            [CzSerialize(6)] public Vector3 v;
            [CzSerialize(7)] public long[] a2;
        }

        [CzCustomSerializeMembers]
        private class Xe {
#pragma warning disable 649
            [CzSerialize(0)] public int w;

            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            [CzSerialize(1)] public int H { get; set; }

            [CzSerialize(2)] public byte[] a;
            [CzSerialize(3)] public List<string> l;
            [CzSerialize(4)] public Dictionary<string, string> d;
            [CzSerialize(5)] public E e;
            [CzSerialize(6)] public Vector3 v;
            [CzSerialize(7)] public long[] a2;
            [CzSerialize(8)] public Vector3 v2;
#pragma warning restore 649
        }

        private class A {
            public int w;
            public int H { get; set; }
            public sbyte[] a;
            public List<string> l;
            public Dictionary<string, string> d;
            public E e;
            public Vector3 v;
            public long[] a2;
        }

        public enum E : short {
            A,
            B,
            C
        }
    }
}