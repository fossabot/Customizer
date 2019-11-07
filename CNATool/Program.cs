using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using CommandLine;
using CommandLine.Text;
using CustomNavi.Authoring;
using CustomNavi.Content;
using CustomNavi.Modeling;
using CustomNavi.Texturing;
using CustomNavi.Utility;

namespace CNATool {
    internal static class Program {
        private static int Main(string[] args) => Parser.Default
            .ParseArguments<TemplateOptions, LiveMeshOptions, BinDefOptions>(args)
            .MapResult(
                (TemplateOptions opts) => RunTemplate(opts),
                (LiveMeshOptions opts) => RunLiveMesh(opts),
                (BinDefOptions opts) => RunBinDef(opts),
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
                ContentUtil.SerializeContentDefinition(def, ofs);
            return 0;
        }

        [Verb("bindef", HelpText = "Generate binary-serialized ContentDefinition from json.")]
        private class BinDefOptions {
            [Value(0, MetaName = "sourceFile", Required = true, HelpText = "Json source file to read.")]
            public string SourceFile { get; set; }

            [Value(1, MetaName = "targetFile", Required = true, HelpText = "Target file to write.")]
            public string TargetFile { get; set; }

            [Usage]
            // ReSharper disable once UnusedMember.Local
            public static IEnumerable<Example> Examples =>
                new List<Example> {
                    new Example("Generate binary-serialized ContentDefinition from json",
                        new BinDefOptions {SourceFile = "<sourceFile>", TargetFile = "<targetFile>"})
                };
        }

        private static int RunBinDef(BinDefOptions options) {
            var def = ContentUtil.DeserializeContentDefinition(new FileStream(options.SourceFile, FileMode.Open, FileAccess.Read));
            using (var ofs = new DeflateStream(new FileStream(options.TargetFile, FileMode.Create, FileAccess.Write),
                CompressionLevel.Optimal, false))
                ContentUtil.SerializeContentDefinition(def, ofs, false);
            return 0;
        }

        [Verb("livemesh", HelpText = "Generate binary-serialized LiveMesh from FBX.")]
        private class LiveMeshOptions {
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
                        new LiveMeshOptions {SourceFile = "<sourceFile>", TargetFile = "<targetFile>"})
                };
        }

        private static int RunLiveMesh(LiveMeshOptions options) {
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
    }
}