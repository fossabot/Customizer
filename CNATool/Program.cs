using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using CommandLine;
using CommandLine.Text;
using CustomNavi.Authoring;
using CustomNavi.Box;
using CustomNavi.Content;
using CustomNavi.Modeling;
using CustomNavi.Texturing;
using CustomNavi.Utility;
using SixLabors.ImageSharp.Formats.Png;

namespace CNATool {
    internal static class Program {
        private static int Main(string[] args) => Parser.Default
            .ParseArguments<TemplateOptions, MakeMeshOptions, PackOptions, UnpackOptions, CompositeOptions>(args)
            .MapResult(
                (TemplateOptions opts) => RunTemplate(opts),
                (MakeMeshOptions opts) => RunMakeMesh(opts),
                (PackOptions opts) => RunPack(opts),
                (UnpackOptions opts) => RunUnpack(opts),
                (CompositeOptions opts) => RunComposite(opts),
                errs => 1);

        [Verb("template", HelpText = "Generate template ContentDefinition json.")]
        private class TemplateOptions {
            [Value(0, MetaName = "targetFile", Required = true, HelpText = "Target file to write.")]
            public string TargetFile { get; set; }

            [Usage]
            // ReSharper disable once UnusedMember.Local
            public static IEnumerable<Example> Examples =>
                new List<Example> {
                    new Example("Generate template ContentDefinition json",
                        new TemplateOptions {TargetFile = "<targetFile>"})
                };
        }

        private static int RunTemplate(TemplateOptions options) {
            var def = new ContentDefinition();
            def.MeshConfigs.Add("(configName)", new MeshConfig {
                Mesh = "(meshName)",
                Materials = new List<Material> {
                    new Material
                        {CoTextures = new Dictionary<string, string> {{"(shaderParameterName)", "(textureName)"}}}
                },
                CustomAttachPoints = new List<AttachPoint> {new AttachPoint {BoneName = "(boneName)"}}
            });
            def.MeshPaths.Add("(meshName)", "(file|deflate://local/...)");
            def.ResourcePaths.Add("(resourceName)", "(file|deflate://local/...)");
            def.TexturePaths.Add("(textureName)", "(file|deflate://local/...)");
            def.TranslationPaths.Add("(translationName)", "(file|deflate://local/...)");
            def.CoTextures.Add("(coTextureName)", new CoTextureDefinition {
                Height = 1024,
                Width = 1024,
                Textures = new List<SubTextureDefinition>
                    {new SubTextureDefinition {Mask = "(coTextureName)", Texture = "(coTextureName)"}}
            });
            using (var ofs = new FileStream(options.TargetFile, FileMode.Create, FileAccess.Write))
                ContentUtil.SerializeJsonContentDefinition(def, ofs);
            return 0;
        }

        [Verb("makemesh", HelpText = "Generate binary-serialized LiveMesh from FBX.")]
        private class MakeMeshOptions {
            [Value(0, MetaName = "sourceFile", Required = true, HelpText = "FBX source file to read.")]
            public string SourceFile { get; set; }

            [Value(1, MetaName = "targetFile", Required = true, HelpText = "Target file to write.")]
            public string TargetFile { get; set; }

            [Option('m', "matchFile", MetaValue = "FILE", HelpText = "Use bone / attach point regex json file.")]
            public string MatchFile { get; set; } = null;

            [Usage]
            // ReSharper disable once UnusedMember.Local
            public static IEnumerable<Example> Examples =>
                new List<Example> {
                    new Example("Generate binary-serialized LiveMesh from FBX",
                        new MakeMeshOptions {SourceFile = "<sourceFile>", TargetFile = "<targetFile>"})
                };
        }

        private static int RunMakeMesh(MakeMeshOptions options) {
            LiveMesh mesh;
            using (var ifs = new FileStream(options.SourceFile, FileMode.Open, FileAccess.Read)) {
                List<Tuple<BoneType, string>> boneRegexs = null;
                List<Tuple<AttachPointType, string>> attachPointRegexs = null;
                if (options.MatchFile != null) {
                    var mFile = new FileInfo(options.MatchFile);
                    if (mFile.Exists) {
                        var doc = JsonDocument.Parse(mFile.OpenRead());
                        var root = doc.RootElement;
                        foreach (var elem in root.EnumerateObject()) {
                            if ("bones".Equals(elem.Name, StringComparison.InvariantCultureIgnoreCase)) {
                                boneRegexs = new List<Tuple<BoneType, string>>();
                                foreach (var elem2 in elem.Value.EnumerateObject()) {
                                    Enum.TryParse(typeof(BoneType), elem2.Name, true, out var type);
                                    boneRegexs.Add(
                                        new Tuple<BoneType, string>((BoneType) type, elem2.Value.GetString()));
                                }
                            }
                            else if ("attachPoints".Equals(elem.Name, StringComparison.InvariantCultureIgnoreCase)) {
                                attachPointRegexs = new List<Tuple<AttachPointType, string>>();
                                foreach (var elem2 in elem.Value.EnumerateObject()) {
                                    Enum.TryParse(typeof(AttachPointType), elem2.Name, true, out var type);
                                    attachPointRegexs.Add(
                                        new Tuple<AttachPointType, string>((AttachPointType) type,
                                            elem2.Value.GetString()));
                                }
                            }
                        }
                    }
                }

                mesh = AuthoringUtil.GenerateLiveMesh(ifs, boneRegexs, attachPointRegexs);
            }

            using (var ofs = new DeflateStream(new FileStream(options.TargetFile, FileMode.Create, FileAccess.Write),
                CompressionLevel.Optimal, false))
                Serializer.Serialize(ofs, mesh);
            return 0;
        }

        [Verb("pack", HelpText = "Write CnBox container.")]
        private class PackOptions {
            [Value(0, MetaName = "targetFile", Required = true, HelpText = "Target file to write.")]
            public string TargetFile { get; set; }

            [Value(1, MetaName = "resources", Required = false, HelpText = "Resource files to pack.")]
            public IEnumerable<string> Resources { get; set; }

            [Option('d', "definition", Required = false, MetaValue = "FILE",
                HelpText = "Json content definition to pack.")]
            public string ContentDefinition { get; set; }

            [Usage]
            // ReSharper disable once UnusedMember.Local
            public static IEnumerable<Example> Examples =>
                new List<Example> {
                    new Example("Write Write CnBox container",
                        new PackOptions {
                            TargetFile = "<targetFile>", ContentDefinition = "<contentDefinition>",
                            Resources = new[] {"[resources...]"}
                        }),
                    new Example("Write CnBox container without content defintion",
                        new PackOptions {
                            TargetFile = "<targetFile>",
                            Resources = new[] {"[resources...]"}
                        })
                };
        }

        private static int RunPack(PackOptions options) {
            ContentDefinition def = null;
            if (options.ContentDefinition != null)
                def = ContentUtil.DeserializeJsonContentDefinition(new FileStream(options.ContentDefinition,
                    FileMode.Open,
                    FileAccess.Read));
            var bList = new List<FileInfo>();
            if (options.Resources != null)
                foreach (var res in options.Resources)
                    bList.Add(new FileInfo(res));
            var metaList = new List<MutableKeyValuePair<string, int>>();
            foreach (var ent in bList)
                metaList.Add(new MutableKeyValuePair<string, int>(ent.Name, (int) ent.Length));
            using (var ofs = new FileStream(options.TargetFile, FileMode.Create, FileAccess.Write)) {
                CnBox.WriteHead(ofs, def, metaList);
                foreach (var ent in bList)
                    using (var eStream = ent.OpenRead())
                        eStream.CopyTo(ofs);
            }

            return 0;
        }

        [Verb("unpack", HelpText = "Unpack CnBox container.")]
        private class UnpackOptions {
            [Value(0, MetaName = "sourceFile", Required = true, HelpText = "Source file to read.")]
            public string SourceFile { get; set; }

            [Value(1, MetaName = "targetDir", Required = true, HelpText = "Target directory.")]
            public string TargetDir { get; set; }

            [Usage]
            // ReSharper disable once UnusedMember.Local
            public static IEnumerable<Example> Examples =>
                new List<Example> {
                    new Example("Unpack CnBox container",
                        new UnpackOptions {
                            SourceFile = "<sourceFile>", TargetDir = "<targetDir>"
                        })
                };
        }

        private static int RunUnpack(UnpackOptions options) {
            Directory.CreateDirectory(options.TargetDir);
            using (var ifs = new FileStream(options.SourceFile, FileMode.Open, FileAccess.Read)) {
                var box = CnBox.Load(ifs);
                var def = box.GetContentDefinition();
                if (def != null)
                    using (var cnBoxFs = new FileStream(Path.Combine(options.TargetDir, "contentDefinition.json"),
                        FileMode.Create, FileAccess.Write))
                        ContentUtil.SerializeJsonContentDefinition(def, cnBoxFs);
                foreach (var (name, srcResFs) in box.GetResourceEnumerator())
                    using (var resFs = new FileStream(Path.Combine(options.TargetDir, name), FileMode.Create,
                        FileAccess.Write))
                        srcResFs.CopyTo(resFs);
            }

            return 0;
        }

        [Verb("composite", HelpText = "Generate composite images from content definition.")]
        private class CompositeOptions {
            [Value(0, MetaName = "contentDefinition", Required = true,
                HelpText = "Source json content definition to read.")]
            public string ContentDefinition { get; set; }

            [Value(1, MetaName = "targetDir", Required = true, HelpText = "Target directory.")]
            public string TargetDir { get; set; }

            [Option('d', "directory", Required = false, MetaValue = "DIRECTORY",
                HelpText = "Custom content directory to use.")]
            public string ContentDir { get; set; }

            [Usage]
            // ReSharper disable once UnusedMember.Local
            public static IEnumerable<Example> Examples =>
                new List<Example> {
                    new Example("Generate composite images from content definition",
                        new CompositeOptions {
                            ContentDefinition = "<sourceFile>", TargetDir = "<targetDir>"
                        }),
                    new Example("Generate composite images from content definition from specified directory",
                        new CompositeOptions {
                            ContentDefinition = "<sourceFile>", TargetDir = "<targetDir>", ContentDir = "<contentDir>"
                        })
                };
        }

        private static int RunComposite(CompositeOptions options) {
            Directory.CreateDirectory(options.TargetDir);
            ContentDefinition cd;
            using (var ifs = new FileStream(options.ContentDefinition, FileMode.Open, FileAccess.Read)) {
                cd = Serializer.Deserialize<ContentDefinition>(ifs);
            }

            var rm = new ResourceManager();
            rm.RegisterResourceProvider(
                new FileResourceProvider(options.ContentDir ?? Path.GetDirectoryName(options.ContentDefinition)));
            var opts = new LiveLoadOptions(loadTextures: true);
            var lc = ContentUtil.LoadLiveContent(cd, rm, opts);
            var pngEncoder = new PngEncoder();
            foreach (var (name, image) in lc.RenderedCoTextures) {
                using (var ofs = new FileStream(Path.Combine(options.TargetDir, name + ".png"), FileMode.Create,
                    FileAccess.Write))
                    image.Save(ofs, pngEncoder);
            }

            return 0;
        }
    }
}