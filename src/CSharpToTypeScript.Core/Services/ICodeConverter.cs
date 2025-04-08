using CSharpToTypeScript.Core.Options;
using System.Collections.Generic;

namespace CSharpToTypeScript.Core.Services
{
    public interface ICodeConverter
    {
        string ConvertToTypeScript(string code, CodeConversionOptions options);
        public List<(string FileName, string TsCode)> ConvertAllToTypeScript(Dictionary<string, string> csFiles, CodeConversionOptions options);
    }
}