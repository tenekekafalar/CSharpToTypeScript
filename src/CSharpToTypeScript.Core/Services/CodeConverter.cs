using CSharpToTypeScript.Core.Options;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Linq;

namespace CSharpToTypeScript.Core.Services
{
    internal class CodeConverter : ICodeConverter
    {
        private readonly SyntaxTreeConverter _syntaxTreeConverter;

        public CodeConverter(SyntaxTreeConverter syntaxTreeConverter)
        {
            _syntaxTreeConverter = syntaxTreeConverter;
        }

        public string ConvertToTypeScript(string code, CodeConversionOptions options)
            => _syntaxTreeConverter.Convert(CSharpSyntaxTree.ParseText(code))
                .WriteTypeScript(options);

        public List<(string FileName, string TsCode)> ConvertAllToTypeScript(Dictionary<string, string> csFiles, CodeConversionOptions options)
        {
            // 1. Parse & Convert all .cs file contents into FileNodes
            var fileNodes = csFiles.Select(kvp =>
            {
                var root = CSharpSyntaxTree.ParseText(kvp.Value,path:kvp.Key);
                var node = _syntaxTreeConverter.Convert(root);
                return (FileName: kvp.Key, FileNode: node);
            }).ToList();

            // 2. Merge all RootNodes for import resolution
            var allRootNodes = fileNodes.SelectMany(f => f.FileNode.RootNodes).ToList();
            options.AllRootNodes = allRootNodes;

            // 3. Generate TS for each
            var results = fileNodes.Select(f =>
                (f.FileName, TsCode: f.FileNode.WriteTypeScript(options))
            ).ToList();

            return results;
        }

    }
}