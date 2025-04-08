using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSharpToTypeScript.Core.Options;
using CSharpToTypeScript.Core.Transformations;
using CSharpToTypeScript.Core.Utilities;
using static CSharpToTypeScript.Core.Utilities.StringUtilities;

namespace CSharpToTypeScript.Core.Models
{
    internal class FileNode
    {
        public FileNode(IEnumerable<RootNode> rootNodes)
        {
            RootNodes = rootNodes;
        }

        public IEnumerable<RootNode> RootNodes { get; }

        public IEnumerable<string> Requires => RootNodes.SelectMany(r => r.Requires).Distinct();

        public IEnumerable<string> Imports => Requires.Except(RootNodes.Select(r => r.Name));

        public string WriteTypeScript(CodeConversionOptions options)
        {
            var context = new Context();
            var currentFilePath = RootNodes.FirstOrDefault()?.SourceFilePath;
            var currentDir = currentFilePath != null ? System.IO.Path.GetDirectoryName(currentFilePath) : null;

            var importLines = new List<string>();

            foreach (var importName in Imports)
            {
                var importedNode = options.AllRootNodes?.FirstOrDefault(r => r.Name == importName);
                var importedFilePath = importedNode?.SourceFilePath;

                string relativePath;

                if (!string.IsNullOrEmpty(currentDir) && !string.IsNullOrEmpty(importedFilePath))
                {
                    var rel = GetRelativePath(currentDir, importedFilePath);
                    relativePath = System.IO.Path.ChangeExtension(rel, null)?.Replace("\\", "/");

                    if (!relativePath.StartsWith("."))
                        relativePath = "./" + relativePath;
                }
                else
                {
                    relativePath = "./" + ModuleNameTransformation.Transform(importName, options);
                }

                var importLine =
                    "import { " + importName.TransformIf(options.RemoveInterfacePrefix, StringUtilities.RemoveInterfacePrefix) + " } from " +
                    relativePath.InQuotes(options.QuotationMark) + ";";

                importLines.Add(importLine);
            }

            return
                (importLines.Distinct().LineByLine() + EmptyLine)
                    .If(importLines.Any() && options.ImportGenerationMode != ImportGenerationMode.None)
                + RootNodes.WriteTypeScript(options, context).ToEmptyLineSeparatedList()
                + NewLine.If(options.AppendNewLine);
        }
        public static string GetRelativePath(string fromPath, string toPath)
        {
            fromPath = Path.GetFullPath(AppendDirectorySeparatorChar(fromPath));
            toPath = Path.GetFullPath(toPath);

            var fromUri = new Uri(fromPath);
            var toUri = new Uri(toPath);

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            return Uri.UnescapeDataString(relativeUri.ToString()).Replace("/", Path.DirectorySeparatorChar.ToString());
        }


        private static string AppendDirectorySeparatorChar(string path)
        {
            if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                return path + Path.DirectorySeparatorChar;
            }

            return path;
        }
    }
}