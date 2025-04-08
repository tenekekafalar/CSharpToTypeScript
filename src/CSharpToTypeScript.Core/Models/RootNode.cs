using System.Collections.Generic;
using System.Linq;
using CSharpToTypeScript.Core.Options;

namespace CSharpToTypeScript.Core.Models
{
    public abstract class RootNode : IWritableNode, IDependentNode
    {
        public abstract string Name { get; }
        
        public virtual string SourceFilePath { get; }

        public virtual IEnumerable<string> Requires => Enumerable.Empty<string>();

        public abstract string WriteTypeScript(CodeConversionOptions options, Context context);
    }
}