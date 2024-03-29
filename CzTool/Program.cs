﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using CommandLine;
using CommandLine.Text;
using Customizer.Authoring;
using Customizer.Box;
using Customizer.Content;
using Customizer.Modeling;
using Customizer.Texturing;
using Customizer.Utility;
using SixLabors.ImageSharp;

namespace CzTool {
    internal static class Program {
        private static int Main(string[] args) => Parser.Default
            .ParseArguments<TemplateOptions, MakeMeshOptions, MakeAnimOptions, MakeTlOptions, PackOptions, UnpackOptions
                , CompositeOptions>(args)
            .MapResult(
                (TemplateOptions opts) => RunTemplate(opts),
                (MakeMeshOptions opts) => RunMakeMesh(opts),
                (MakeAnimOptions opts) => RunMakeAnim(opts),
                (MakeTlOptions opts) => RunMakeTl(opts),
                (PackOptions opts) => RunPack(opts),
                (UnpackOptions opts) => RunUnpack(opts),
                (CompositeOptions opts) => RunComposite(opts),
                errs => 1);

        [Verb("template", HelpText = "Generate template file.")]
        private class TemplateOptions {
            [Value(0, MetaName = "type", Required = true, HelpText = "Type of template (definition, matchfile).")]
            public string Type { get; set; }

            [Value(1, MetaName = "targetFile", Required = true, HelpText = "Target file to write.")]
            public string TargetFile { get; set; }

            [Usage]
            // ReSharper disable once UnusedMember.Local
            public static IEnumerable<Example> Examples =>
                new List<Example> {
                    new Example("Generate template ContentDefinition JSON",
                        new TemplateOptions {Type = "definition", TargetFile = "<targetFile>"}),
                    new Example("Generate template matchFile JSON",
                        new TemplateOptions {Type = "matchfile", TargetFile = "<targetFile>"})
                };
        }

        private static int RunTemplate(TemplateOptions options) {
            switch (options.Type.ToLowerInvariant()) {
                case "definition":
                    var def = new ContentDefinition();
                    def.MeshConfigs.Add("yourConfigName", new MeshConfig {
                        Mesh = "yourMeshName",
                        Materials = new List<Material> {
                            new Material {
                                Textures = new Dictionary<string, string>
                                    {{"yourShaderParameterName", "yourTextureName"}}
                            }
                        },
                        CustomAttachPoints = new List<AttachPoint> {new AttachPoint {BoneName = "yourBoneName"}}
                    });
                    def.MeshPaths.Add("yourMeshName", "yourPath");
                    def.ResourcePaths.Add("yourResourceName", "yourPath");
                    def.TexturePaths.Add("yourTextureName", "yourPath");
                    def.TranslationPaths.Add("yourTranslationName", "yourPath");
                    def.CoTextures.Add("yourCoTextureName", new CoTextureDefinition {
                        Height = 1024,
                        Width = 1024,
                        Textures = new List<SubTextureDefinition>
                            {new SubTextureDefinition {Mask = "yourTextureName", Texture = "yourTextureName"}}
                    });
                    using (var ofs = new FileStream(options.TargetFile, FileMode.Create, FileAccess.Write))
                        AuthoringUtil.JsonSerialize(def, ofs);
                    break;
                case "matchfile":
                    var matchFile = new MatchRules();
                    foreach (var t in (BoneType[]) Enum.GetValues(typeof(BoneType)))
                        matchFile.Bones.Add($"boneRegex{t.ToString()}", t);

                    foreach (var t in (AttachPointType[]) Enum.GetValues(typeof(AttachPointType)))
                        matchFile.AttachPoints.Add($"boneRegex{t.ToString()}", t);

                    using (var ofs = new FileStream(options.TargetFile, FileMode.Create, FileAccess.Write))
                        AuthoringUtil.JsonSerialize(matchFile, ofs);
                    break;
                default:
                    Console.Error.WriteLine("Template type not recognized");
                    return 0x10103040;
            }

            return 0;
        }

        [Verb("makemesh", HelpText = "Generate LiveMesh from FBX.")]
        private class MakeMeshOptions {
            [Value(0, MetaName = "sourceFile", Required = true, HelpText = "FBX source file to read.")]
            public string SourceFile { get; set; }

            [Value(1, MetaName = "targetFile", Required = true, HelpText = "Target file to write.")]
            public string TargetFile { get; set; }

            [Option('m', "matchFile", MetaValue = "FILE", HelpText = "Use bone / attach point regex JSON file.")]
            public string MatchFile { get; set; } = null;

            [Option('u', "uniqueName", MetaValue = "NAME", HelpText = "Set this mesh's unique name.")]
            public string UniqueName { get; set; } = null;

            [Option('v', "variantTypeName", MetaValue = "NAME", HelpText = "Set the base variant type name.")]
            public string VariantTypeName { get; set; } = null;

            [Option('f', "fitUniqueName", MetaValue = "NAME",
                HelpText = "Set the unique name of the mesh originally fit against.")]
            public string FitUniqueName { get; set; } = null;

            [Option('c', "compress", HelpText = "Deflate-compress output.")]
            public bool Compress { get; set; } = false;

            [Option('d', "debugLog", HelpText = "Print loader debug log messages.")]
            public bool DebugLog { get; set; } = false;

            [Usage]
            // ReSharper disable once UnusedMember.Local
            public static IEnumerable<Example> Examples =>
                new List<Example> {
                    new Example("Generate LiveMesh from FBX",
                        new MakeMeshOptions {SourceFile = "<sourceFile>", TargetFile = "<targetFile>"})
                };
        }

        private static int RunMakeMesh(MakeMeshOptions options) {
            LiveMesh mesh;
            using (var ifs = new FileStream(options.SourceFile, FileMode.Open, FileAccess.Read)) {
                Dictionary<string, BoneType> boneRegexs = null;
                Dictionary<string, AttachPointType> attachPointRegexs = null;
                if (options.MatchFile != null) {
                    MatchRules mf;
                    using (var mfFs = new FileStream(options.MatchFile, FileMode.Open, FileAccess.Read))
                        mf = AuthoringUtil.JsonDeserialize<MatchRules>(mfFs);
                    boneRegexs = mf.Bones;
                    attachPointRegexs = mf.AttachPoints;
                }

                Console.WriteLine("Parsing...");
                mesh = options.DebugLog
                    ? AuthoringUtil.GenerateLiveMesh(ifs, boneRegexs, attachPointRegexs, Console.WriteLine)
                    : AuthoringUtil.GenerateLiveMesh(ifs, boneRegexs, attachPointRegexs);
            }

            mesh.UniqueName = options.UniqueName;
            mesh.VariantTypeName = options.VariantTypeName;
            mesh.FitUniqueName = options.FitUniqueName;

            Stream ofs = new FileStream(options.TargetFile, FileMode.Create, FileAccess.Write);
            try {
                if (options.Compress)
                    ofs = new DeflateStream(ofs, CompressionLevel.Optimal, false);
                Console.WriteLine("Serializing...");
                Serializer.Serialize(ofs, mesh);
            }
            finally {
                ofs.Dispose();
            }

            return 0;
        }

        [Verb("makeanim", HelpText = "Generate LiveAnim from FBX.")]
        private class MakeAnimOptions {
            [Value(0, MetaName = "sourceFile", Required = true, HelpText = "FBX source file to read.")]
            public string SourceFile { get; set; }

            [Value(1, MetaName = "targetFile", Required = true, HelpText = "Target file to write.")]
            public string TargetFile { get; set; }

            [Option('m', "matchFile", MetaValue = "FILE", HelpText = "Use bone regex JSON file.")]
            public string MatchFile { get; set; } = null;

            [Option('s', "sourceUniqueName", MetaValue = "NAME",
                HelpText = "Set the unique name of the mesh originally animated against.")]
            public string SourceUniqueName { get; set; } = null;

            [Option('c', "compress", HelpText = "Deflate-compress output.")]
            public bool Compress { get; set; } = false;

            [Option('d', "debugLog", HelpText = "Print loader debug log messages.")]
            public bool DebugLog { get; set; } = false;

            [Usage]
            // ReSharper disable once UnusedMember.Local
            public static IEnumerable<Example> Examples =>
                new List<Example> {
                    new Example("Generate LiveAnim from FBX",
                        new MakeAnimOptions {SourceFile = "<sourceFile>", TargetFile = "<targetFile>"})
                };
        }

        private static int RunMakeAnim(MakeAnimOptions options) {
            LiveAnim anim;
            using (var ifs = new FileStream(options.SourceFile, FileMode.Open, FileAccess.Read)) {
                Dictionary<string, BoneType> boneRegexs = null;
                if (options.MatchFile != null) {
                    MatchRules mf;
                    using (var mfFs = new FileStream(options.MatchFile, FileMode.Open, FileAccess.Read))
                        mf = AuthoringUtil.JsonDeserialize<MatchRules>(mfFs);
                    boneRegexs = mf.Bones;
                }

                Console.WriteLine("Parsing...");
                anim = options.DebugLog
                    ? AuthoringUtil.GenerateLiveAnim(ifs, boneRegexs, Console.WriteLine)
                    : AuthoringUtil.GenerateLiveAnim(ifs, boneRegexs);
            }

            anim.SourceUniqueName = options.SourceUniqueName;

            Stream ofs = new FileStream(options.TargetFile, FileMode.Create, FileAccess.Write);
            try {
                if (options.Compress)
                    ofs = new DeflateStream(ofs, CompressionLevel.Optimal, false);
                Console.WriteLine("Serializing...");
                Serializer.Serialize(ofs, anim);
            }
            finally {
                ofs.Dispose();
            }

            return 0;
        }

        [Verb("maketl", HelpText = "Generate translation dictionary from JSON.")]
        private class MakeTlOptions {
            [Value(0, MetaName = "sourceFile", Required = true, HelpText = "JSON source file to read.")]
            public string SourceFile { get; set; }

            [Value(1, MetaName = "targetFile", Required = true, HelpText = "Target file to write.")]
            public string TargetFile { get; set; }

            [Option('c', "compress", HelpText = "Deflate-compress output.")]
            public bool Compress { get; set; } = false;

            [Usage]
            // ReSharper disable once UnusedMember.Local
            public static IEnumerable<Example> Examples =>
                new List<Example> {
                    new Example("Generate translation dictionary from JSON",
                        new MakeTlOptions {SourceFile = "<sourceFile>", TargetFile = "<targetFile>"})
                };
        }

        private static int RunMakeTl(MakeTlOptions options) {
            Dictionary<string, string> dict;
            using (var ifs = new FileStream(options.SourceFile, FileMode.Open, FileAccess.Read))
                dict = AuthoringUtil.JsonDeserialize<Dictionary<string, string>>(ifs);
            Stream ofs = new FileStream(options.TargetFile, FileMode.Create, FileAccess.Write);
            try {
                if (options.Compress)
                    ofs = new DeflateStream(ofs, CompressionLevel.Optimal, false);
                Console.WriteLine("Serializing...");
                Serializer.Serialize(ofs, dict);
            }
            finally {
                ofs.Dispose();
            }

            return 0;
        }

        [Verb("pack", HelpText = "Write CnBox container.")]
        private class PackOptions {
            [Value(0, MetaName = "targetFile", Required = true, HelpText = "Target file to write.")]
            public string TargetFile { get; set; }

            [Value(1, MetaName = "resources", Required = false, HelpText = "Resource files to pack.")]
            public IEnumerable<string> Resources { get; set; }

            [Option('d', "definition", Required = false, MetaValue = "FILE",
                HelpText = "JSON content definition to pack.")]
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
                    new Example("Write CnBox container without content definition",
                        new PackOptions {
                            TargetFile = "<targetFile>",
                            Resources = new[] {"[resources...]"}
                        })
                };
        }

        private static int RunPack(PackOptions options) {
            ContentDefinition def = null;
            if (options.ContentDefinition != null) {
                using (var defFs = new FileStream(options.ContentDefinition, FileMode.Open, FileAccess.Read))
                    def = AuthoringUtil.JsonDeserialize<ContentDefinition>(defFs);
            }

            var bList = new List<FileInfo>();
            if (options.Resources != null)
                foreach (var res in options.Resources)
                    bList.Add(new FileInfo(res));
            var metaList = new List<MutableKeyValuePair<string, int>>();
            foreach (var ent in bList)
                metaList.Add(new MutableKeyValuePair<string, int>(ent.Name, (int) ent.Length));
            using (var ofs = new FileStream(options.TargetFile, FileMode.Create, FileAccess.Write)) {
                CzBox.WriteHead(ofs, def, metaList);
                foreach (var ent in bList)
                    using (var eStream = ent.OpenRead())
                        eStream.CopyTo(ofs);
            }

            return 0;
        }

        [Verb("composite", HelpText = "Generate composite images from content definition.")]
        private class CompositeOptions {
            [Value(0, MetaName = "contentDefinition", Required = true,
                HelpText = "Source JSON content definition to read.")]
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
                cd = AuthoringUtil.JsonDeserialize<ContentDefinition>(ifs);
            }

            var rm = new DataManager();
            rm.RegisterDataProvider(
                new FileDataProvider(options.ContentDir ?? Path.GetDirectoryName(options.ContentDefinition)));
            var opts = new LiveLoadOptions(loadTextures: true);
            var lc = ContentUtil.LoadLiveContent(cd, rm, opts);
            foreach (var (name, image) in lc.RenderedCoTextures) {
                using (var ofs = new FileStream(Path.Combine(options.TargetDir, name + ".png"), FileMode.Create,
                    FileAccess.Write))
                    image.SaveAsPng(ofs);
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
                var box = CzBox.Load(ifs);
                var def = box.GetContentDefinition();
                if (def != null)
                    using (var cnBoxFs = new FileStream(Path.Combine(options.TargetDir, "contentDefinition.json"),
                        FileMode.Create, FileAccess.Write))
                        AuthoringUtil.JsonSerialize(def, cnBoxFs);
                foreach (var (name, srcResFs) in box.GetDataEnumerator())
                    using (var resFs = new FileStream(Path.Combine(options.TargetDir, name), FileMode.Create,
                        FileAccess.Write))
                        srcResFs.CopyTo(resFs);
            }

            return 0;
        }

        public class MatchRules {
            public Dictionary<string, BoneType> Bones { get; set; } = new Dictionary<string, BoneType>();

            public Dictionary<string, AttachPointType> AttachPoints { get; set; } =
                new Dictionary<string, AttachPointType>();
        }
    }
}