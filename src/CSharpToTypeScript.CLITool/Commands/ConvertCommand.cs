using System;
using System.IO;
using System.Linq;
using CSharpToTypeScript.CLITool.Conventions;
using CSharpToTypeScript.CLITool.Utilities;
using CSharpToTypeScript.Core.Options;
using CSharpToTypeScript.Core.Services;
using McMaster.Extensions.CommandLineUtils;

namespace CSharpToTypeScript.CLITool.Commands
{
    [Command(Name = "dotnet cs2ts", Description = "Convert C# Models, ViewModels and DTOs into their TypeScript equivalents"),
    Subcommand(typeof(InitializeCommand))]
    public class ConvertCommand : CommandBase
    {
        private readonly ICodeConverter _codeConverter;
        private readonly IFileNameConverter _fileNameConverter;

        public ConvertCommand(ICodeConverter codeConverter, IFileNameConverter fileNameConverter)
        {
            _codeConverter = codeConverter;
            _fileNameConverter = fileNameConverter;
        }

        public CodeConversionOptions CodeConversionOptions
            => new CodeConversionOptions(!SkipExport, UseTabs, TabSize, ConvertDatesTo, ConvertNullablesTo,
                !PreserveCasing, !PreserveInterfacePrefix,
                ImportGeneration, UseKebabCase, AppendModelSuffix, QuotationMark, AppendNewLine);

        public void OnExecute()
        {
            if (AngularMode)
            {
                AngularConventions.Override(this);
            }

            if (ClearOutputDirectory && !string.IsNullOrWhiteSpace(Output)
            && Directory.Exists(Output) && !Output.IsSameOrParrentDirectory(Input))
            {
                Directory.Delete(Output, true);
            }

            if (Input.EndsWithFileExtension() && File.Exists(Input))
            {
                OnInputIsFile();
            }
            else if (!Input.EndsWithFileExtension() && Directory.Exists(Input))
            {
                OnInputIsDirectory();
            }
        }

        private void OnInputIsFile()
        {
            var content = File.ReadAllText(Input);
            var converted = _codeConverter.ConvertToTypeScript(content, CodeConversionOptions);
            var outputPath = GetOutputFilePath(Input, Output, CodeConversionOptions);

            CreateOrUpdateFile(outputPath, converted, PartialOverride);
        }

        private void OnInputIsDirectory()
        {
            // 1. .cs dosyalarýný oku
            var fileMap = FileSystem.GetFilesWithExtension(Input, "cs")
                .ToDictionary(f => f, f => File.ReadAllText(f));

            // 2. AllRootNodes bilgisi ile toplu çeviri yap
            var results = _codeConverter.ConvertAllToTypeScript(fileMap, CodeConversionOptions);

            // 3. Ayný OutputPath hesaplamayý kullan
            foreach (var (inputPath, tsCode) in results)
            {
                var outputPath = GetOutputFilePath(
                    inputPath,
                    Output + Path.GetRelativePath(Input, Path.GetDirectoryName(inputPath)),
                    CodeConversionOptions);

                if (!string.IsNullOrWhiteSpace(tsCode))
                    CreateOrUpdateFile(outputPath, tsCode, PartialOverride);
            }
        }

        private void CreateOrUpdateFile(string path, string content, bool partialOverride)
        {
            Directory.CreateDirectory(path.ContainingDirectory());

            if (partialOverride)
            {
                content = Marker.Update(File.Exists(path) ? File.ReadAllText(path) : string.Empty, content);
            }

            File.WriteAllText(path, content);
        }

        private string GetOutputFilePath(string input, string output, ModuleNameConversionOptions options)
            => !input.EndsWithFileExtension() ? throw new ArgumentException("Input should end with file extension.")
            : output?.EndsWithFileExtension() == true ? output
            : !string.IsNullOrWhiteSpace(output) ? Path.Join(output, _fileNameConverter.ConvertToTypeScript(input, options))
            : Path.Join(input.ContainingDirectory(), _fileNameConverter.ConvertToTypeScript(input, options));
    }
}