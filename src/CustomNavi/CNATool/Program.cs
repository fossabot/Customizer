using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace CNATool {
    internal static class Program {
        private static int Main(string[] args) => Parser.Default.ParseArguments<LivMesOptions, BinDefOptions>(args)
            .MapResult(
                (LivMesOptions opts) => RunLivMes(opts),
                (BinDefOptions opts) => RunBinDef(opts),
                errs => 1);

        [Verb("bindef", HelpText = "Generate binary-serialized ContentDefinition from json.")]
        private class BinDefOptions {
            [Value(0, MetaName = "sourceFile", Required = true, HelpText = "Json source file to read.")]
            public string SourceFile { get; set; }

            [Value(1, MetaName = "targetFile", Required = true, HelpText = "Target file to write.")]
            public string TargetFile { get; set; }

            [Usage]
            public static IEnumerable<Example> Examples =>
                new List<Example> {
                    new Example("Generate binary-serialized ContentDefinition from json",
                        new BinDefOptions() {SourceFile = "<sourceFile>", TargetFile = "<targetFile>"})
                };
        }

        private static int RunBinDef(BinDefOptions options) {
            /*
             * TODO verb: bindef
             */
            return 0;
        }

        [Verb("livmes", HelpText = "Generate binary-serialized LiveMesh from FBX.")]
        private class LivMesOptions {
            [Value(0, MetaName = "sourceFile", Required = true, HelpText = "FBX source file to read.")]
            public string SourceFile { get; set; }

            [Value(1, MetaName = "targetFile", Required = true, HelpText = "Target file to write.")]
            public string TargetFile { get; set; }

            [Option('m', "matchFile", MetaValue = "FILE", HelpText = "Use bone / attach point regex json file.")]
            public string MatchFile { get; set; } = null;

            [Usage]
            public static IEnumerable<Example> Examples =>
                new List<Example> {
                    new Example("Generate binary-serialized LiveMesh from FBX",
                        new LivMesOptions() {SourceFile = "<sourceFile>", TargetFile = "<targetFile>"})
                };
        }

        private static int RunLivMes(LivMesOptions options) {
            /*
             * TODO verb: livmes
             * FBX import, LiveMesh serialized export
             * Optional json regex file (bone names, attach points)
             */
            return 0;
        }
    }
}