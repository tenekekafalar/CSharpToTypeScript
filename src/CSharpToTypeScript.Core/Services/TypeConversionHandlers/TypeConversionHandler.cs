using CSharpToTypeScript.Core.Models.TypeNodes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpToTypeScript.Core.Services.TypeConversionHandlers
{
    internal abstract class TypeConversionHandler
    {
        private TypeConversionHandler _nextHandler;

        public virtual ITypeNode Handle(TypeSyntax type)
           => _nextHandler is null ? new Any() : _nextHandler.Handle(type);

        public TypeConversionHandler SetNext(TypeConversionHandler converter)
            => _nextHandler = converter;
    }
}