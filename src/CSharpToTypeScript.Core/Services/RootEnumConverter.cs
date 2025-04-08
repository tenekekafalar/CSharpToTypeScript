using System.Collections.Generic;
using System.Linq;
using CSharpToTypeScript.Core.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpToTypeScript.Core.Services
{
    internal class RootEnumConverter
    {
        public RootEnumNode Convert(EnumDeclarationSyntax @enum,string filePath)
            => new RootEnumNode(
                name: @enum.Identifier.ValueText,
                members: ConvertEnumMembers(@enum.Members),
                sourceFilePath:filePath);

        private IEnumerable<EnumMemberNode> ConvertEnumMembers(IEnumerable<EnumMemberDeclarationSyntax> members)
          => members.Select(m => new EnumMemberNode(
                name: m.Identifier.ValueText,
                value: m.EqualsValue?.Value.ToString()));
    }
}