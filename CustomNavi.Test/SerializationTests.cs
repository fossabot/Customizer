using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using CustomNavi.Utility;
using NUnit.Framework;

namespace CustomNavi.Test {
    public class SerializationTests {
        [SetUp]
        public void Setup() {
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
                v = new Vector3(1.0f, 2.0f, 3.0f)
            };
            Serializer.Serialize(ms, y);
            var span = ms.ToArray().AsSpan();
            var len = Serializer.Deserialize(span, out A z);
            Assert.AreEqual(ms.Length, len, "Serialize/Deserialize length check fail");
            Assert.AreEqual(y.w, z.w, "field check fail");
            Assert.AreEqual(y.H, z.H, "property check fail");
            Assert.IsNotNull(z.a, "array null check fail");
            for (var i = 0; i < y.a.Length; i++)
                Assert.AreEqual(y.a[i], z.a[i], $"array entry {i + 1} check fail");
            Assert.IsNotNull(z.l, "list null check fail");
            Assert.AreEqual(y.a.Length, z.a.Length, "array length check fail");
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
                v = new Vector3(1.0f, 2.0f, 3.0f)
            };
            Serializer.Serialize(ms, y);
            var span = ms.ToArray().AsSpan();
            var len = Serializer.Deserialize(span, out X z);
            Assert.AreEqual(ms.Length, len, "Serialize/Deserialize length check fail");
            Assert.AreEqual(y.w, z.w, "field check fail");
            Assert.AreEqual(y.H, z.H, "property check fail");
            Assert.IsNotNull(z.a, "array null check fail");
            for (var i = 0; i < y.a.Length; i++)
                Assert.AreEqual(y.a[i], z.a[i], $"array entry {i + 1} check fail");
            Assert.IsNotNull(z.l, "list null check fail");
            Assert.AreEqual(y.a.Length, z.a.Length, "array length check fail");
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
                v = new Vector3(1.0f, 2.0f, 3.0f)
            };
            Serializer.Serialize(ms, y);
            var span = ms.ToArray().AsSpan();
            var len = Serializer.Deserialize(span, out Xe z);
            Assert.AreEqual(ms.Length, len, "Serialize/Deserialize length check fail");
            Assert.AreEqual(y.w, z.w, "field check fail");
            Assert.AreEqual(y.H, z.H, "property check fail");
            Assert.IsNotNull(z.a, "array null check fail");
            for (var i = 0; i < y.a.Length; i++)
                Assert.AreEqual(y.a[i], z.a[i], $"array entry {i + 1} check fail");
            Assert.IsNotNull(z.l, "list null check fail");
            Assert.AreEqual(y.a.Length, z.a.Length, "array length check fail");
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

        [NCustomSerializeMembers]
        private class X {
            [NSerialize(0)] public int w;
            [NSerialize(1)] public int H { get; set; }

            [NSerialize(2)] public byte[] a;
            [NSerialize(3)] public List<string> l;
            [NSerialize(4)] public Dictionary<string, string> d;
            [NSerialize(5)] public E e;
            [NSerialize(6)] public Vector3 v;
        }

        [NCustomSerializeMembers]
        private class Xe {
#pragma warning disable 649
            [NSerialize(0)] public int w;

            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            [NSerialize(1)] public int H { get; set; }

            [NSerialize(2)] public byte[] a;
            [NSerialize(3)] public List<string> l;
            [NSerialize(4)] public Dictionary<string, string> d;
            [NSerialize(5)] public E e;
            [NSerialize(6)] public Vector3 v;
            [NSerialize(7)] public Vector3 v2;
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
        }

        public enum E : short {
            A,
            B,
            C
        }
    }
}