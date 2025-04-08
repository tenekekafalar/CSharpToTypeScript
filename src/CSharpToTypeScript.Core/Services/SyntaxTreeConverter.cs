using System;
using System.Collections.Generic;
using System.Linq;
using CSharpToTypeScript.Core.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpToTypeScript.Core.Services
{
    internal class SyntaxTreeConverter
    {
        private readonly RootTypeConverter _rootTypeConverter;
        private readonly RootEnumConverter _rootEnumConverter;

        public SyntaxTreeConverter(RootTypeConverter rootTypeConverter, RootEnumConverter rootEnumConverter)
        {
            _rootTypeConverter = rootTypeConverter;
            _rootEnumConverter = rootEnumConverter;
        }

        public FileNode Convert(SyntaxTree syntaxTree)
        {
            var root = syntaxTree.GetCompilationUnitRoot();
            var filePath = syntaxTree.FilePath;
            return new FileNode(ConvertRootNodes(root, filePath));
        }

        private IEnumerable<RootNode> ConvertRootNodes(CompilationUnitSyntax root, string filePath)
            => root.DescendantNodes()
                .Where(node => (node is TypeDeclarationSyntax type && IsNotStatic(type)) || node is EnumDeclarationSyntax)
                .Select(node => node switch
                {
                    TypeDeclarationSyntax type => (RootNode)_rootTypeConverter.Convert(type, filePath),
                    EnumDeclarationSyntax @enum => _rootEnumConverter.Convert(@enum, filePath),
                    _ => throw new ArgumentException("Unknown syntax type.")
                })
                .ToList();

        private bool IsNotStatic(TypeDeclarationSyntax type)
            => type.Modifiers.All(m => m.Kind() != SyntaxKind.StaticKeyword);
    }
}
