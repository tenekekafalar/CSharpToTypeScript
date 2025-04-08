using CSharpToTypeScript.Core.Options;

namespace CSharpToTypeScript.Core.Models.TypeNodes
{
    internal class ByteArray : TypeNode
    {
        public override string WriteTypeScript(CodeConversionOptions options, Context context) => "Uint8Array";
    }
}