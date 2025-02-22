namespace Raffinert.AccessGenerator.Core;

using DotNet.Globbing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

internal static class FileAccessGenerator
{
	public static void GenerateCode(SourceProductionContext context, GenerationContext generationContext, ResourceKind kind)
	{
		GlobOptions.Default.Evaluation.CaseInsensitive = true;

		var contentFiles = generationContext.With(kind);

		if (contentFiles.IsEmpty)
		{
			return;
		}

		var kindString = kind.ToString();
		var kindLowerCase = kindString.ToLowerInvariant();
		var getItemPathMethodName = $"Get{kindString}FilePath";

		StringBuilder sourceBuilder = new();
		sourceBuilder.AppendLine($"""
		                           #nullable enable
		                           namespace {contentFiles.RootNamespace};
		                           using System;
		                           using System.IO;
		                           using System.Threading;
		                           using System.Threading.Tasks;
		                           """);

		if (contentFiles.IsXunitDataAttributeAvailable)
		{

			sourceBuilder.AppendLine("""
			                         using System.Reflection;
			                         using Xunit.Sdk;
			                         """);

		}

		sourceBuilder.AppendLine($$"""
		                           
		                           /// <summary>
		                           /// Auto-generated class to access all {{kindLowerCase}} files in an assembly.
		                           /// </summary>
		                           public static partial class {{kindString}}s
		                           {
		                               /// <summary>
		                               /// Retrieves a collection of {{kindLowerCase}} files that match the specified pattern literal.
		                               /// </summary>
		                               /// <param name="pattern">The search pattern literal to match {{kindLowerCase}} files.</param>
		                               /// <returns>
		                               /// An <see cref="IEnumerable{{{kindString}}}"/> containing the matched {{kindLowerCase}} files.
		                               /// </returns>
		                               public static IEnumerable<{{kindString}}> GetMatches(string pattern)
		                               {
		                           """);

		var parsedGlobs = contentFiles.MatchesLiterals
			.Distinct()
			.Select(ParsedGlob.Create)
			.Where(x => x.IsCorrect)
			.ToArray();

		var matchedFiles = parsedGlobs
			.Select(g => (g.Pattern, Matches: contentFiles.Where(c => g.Glob.IsMatch(c.RelativePath)).ToArray()))
			.Where(x => x.Matches.Length > 0)
			.OrderBy(x => x.Pattern)
			.ToArray();


		if (matchedFiles.Length == 0)
		{
			sourceBuilder.AppendLine($$"""
									       return Enumerable.Empty<{{kindString}}>();
									    }
									""");
		}
		else
		{
			(HashSet<string> values, string[] patterns)[] matchedFilesGroupedByMatches = matchedFiles.GroupBy(x =>
					new HashSet<string>(x.Matches.Select(m => m.RelativePath)), HashSet<string>.CreateSetComparer())
				.Select(g => (values: g.Key, patterns: g.Select(x => x.Pattern).ToArray()))
				.ToArray();

			sourceBuilder.AppendLine("""
									         switch (pattern)
									         {
									 """);

			foreach (var (matches, patterns) in matchedFilesGroupedByMatches)
			{
				foreach (var pattern in patterns)
				{
					sourceBuilder.AppendLine($"""
					                                      case @"{pattern}": 
					                          """);
				}

				foreach (var match in matches)
				{
					sourceBuilder.AppendLine($"                yield return {kindString}.{Utils.PathAsClassname(match, "_")};");
				}

				sourceBuilder.AppendLine("                break;");
			}

			sourceBuilder.AppendLine("""
			                                 }
			                             }
			                             
			                         """);
		}

		if (contentFiles.IsXunitDataAttributeAvailable)
		{

			sourceBuilder.AppendLine($$"""
			                             /// <summary>
			                             /// An xUnit attribute designed for use with the [Theory] attribute, which returns a collection of {{kindLowerCase}} files matching the specified pattern literal.
			                             /// </summary>
			                             /// <param name="pattern">The search pattern literal to match {{kindLowerCase}} files.</param>
			                             public class FromPatternAttribute(string pattern) : DataAttribute
			                             {
			                                 public override IEnumerable<object[]> GetData(MethodInfo testMethod)
			                                 {
			                                     var matches = GetMatches(pattern);
			                                     return matches.Select(none => new object[] { none });
			                                 }
			                             }
			                         
			                         """);

		}

		sourceBuilder.AppendLine($$"""
		                           	/// <summary>
		                           	/// Gets the {{kindLowerCase}} file's stream.
		                           	/// </summary>
		                           	/// <param name="file">The {{kindLowerCase}} file to retrieve the stream for.</param>
		                           	/// <returns>The stream to access the {{kindLowerCase}} file.</returns>
		                           	public static Stream GetStream(this {{kindString}} file)
		                           	{
		                           	    return File.OpenRead({{getItemPathMethodName}}(file))!;
		                           	}
		                           
		                           	/// <summary>
		                           	/// Gets the {{kindLowerCase}} file's stream-reader.
		                           	/// </summary>
		                           	/// <param name="file">The {{kindLowerCase}} file to retrieve the stream-reader for.</param>
		                           	/// <returns>The stream-reader to access the {{kindLowerCase}} file.</returns>
		                           	public static StreamReader GetReader(this {{kindString}} file)
		                           	{
		                           	    return new StreamReader(File.OpenRead({{getItemPathMethodName}}(file))!, leaveOpen:false);
		                           	}
		                           	
		                           	/// <summary>
		                           	/// Reads the {{kindLowerCase}} file's bytes asynchronously.
		                           	/// </summary>
		                           	/// <param name="file">The {{kindLowerCase}} file to read all bytes.</param>
		                           	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		                           	/// <returns>bytes.</returns>
		                           	public static async Task<byte[]> ReadAllBytesAsync(this {{kindString}} file, CancellationToken cancellationToken = default(CancellationToken))
		                           	{
		                           	    return await File.ReadAllBytesAsync({{getItemPathMethodName}}(file), cancellationToken)!;
		                           	}
		                           	
		                           	/// <summary>
		                           	/// Reads the {{kindLowerCase}} file's bytes.
		                           	/// </summary>
		                           	/// <param name="file">The {{kindLowerCase}} file to read all bytes.</param>
		                           	/// <returns>bytes.</returns>
		                           	public static byte[] ReadAllBytes(this {{kindString}} file)
		                           	{
		                           	    return File.ReadAllBytes({{getItemPathMethodName}}(file))!;
		                           	}
		                           	
		                           	/// <summary>
		                           	/// Reads the {{kindLowerCase}} file's text asynchronously.
		                           	/// </summary>
		                           	/// <param name="file">The {{kindLowerCase}} file to read all text.</param>
		                           	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		                           	/// <returns>text.</returns>
		                           	public static async Task<string> ReadAllTextAsync(this {{kindString}} file, CancellationToken cancellationToken = default(CancellationToken))
		                           	{
		                           	    return await File.ReadAllTextAsync({{getItemPathMethodName}}(file), cancellationToken)!;
		                           	}
		                           	
		                           	/// <summary>
		                           	/// Reads the {{kindLowerCase}} file's text.
		                           	/// </summary>
		                           	/// <param name="file">The {{kindLowerCase}} file to read all text.</param>
		                           	/// <returns>text.</returns>
		                           	public static string ReadAllText(this {{kindString}} file)
		                           	{
		                           	    return File.ReadAllText({{getItemPathMethodName}}(file))!;
		                           	}
		                           	
		                           	/// <summary>
		                           	/// Gets the {{kindLowerCase}} file's FileInfo.
		                           	/// </summary>
		                           	/// <param name="file">The {{kindLowerCase}} file to retrieve the FileInfo for.</param>
		                           	/// <returns>The FileInfo to access the {{kindLowerCase}} file.</returns>
		                           	public static FileInfo GetFileInfo(this {{kindString}} file)
		                           	{
		                           	    return new FileInfo({{getItemPathMethodName}}(file));
		                           	}
		                           	
		                           """);

		sourceBuilder.AppendLine($$"""
		                           	/// <summary>
		                           	/// Gets the {{kindLowerCase}} file's path.
		                           	/// </summary>
		                           	/// <param name="file">The {{kindLowerCase}} file to retrieve the name for.</param>
		                           	/// <returns>The path to access the {{kindLowerCase}} file.</returns>
		                           	public static string {{getItemPathMethodName}}(this {{kindString}} file)
		                           	{
		                           	    return file switch 
		                           	    {
		                           """);

		foreach ((string path, string identifierName, string _, _) in contentFiles)
		{
			sourceBuilder.AppendLine($$"""
			                           	        {{kindString}}.{{identifierName}} => @"{{path}}",
			                           """);
		}

		sourceBuilder.AppendLine("""            _ => throw new InvalidOperationException(),""");

		sourceBuilder.AppendLine("        };");

		sourceBuilder.AppendLine("    }");

		foreach (IGrouping<string, ResourceItem> pathGrouped in contentFiles.GroupBy(g =>
					 Path.GetDirectoryName(g.RelativePath)))
		{
			string pathAsClassName = Utils.PathAsClassname(pathGrouped.Key, "_");
			if (!string.IsNullOrEmpty(pathGrouped.Key))
			{
				sourceBuilder.AppendLine($$"""
				                           
				                           	/// <summary>
				                           	/// Gets the {{kindLowerCase}} file's stream.
				                           	/// </summary>
				                           	/// <param name="file">The {{kindLowerCase}} file to retrieve the stream for.</param>
				                           	/// <returns>The stream to access the {{kindLowerCase}} file.</returns>
				                           	public static Stream GetStream(this {{kindString}}_{{pathAsClassName}} file)
				                           	{
				                           	    return File.OpenRead({{getItemPathMethodName}}(file))!;
				                           	}
				                           
				                           	/// <summary>
				                           	/// Gets the {{kindLowerCase}} file's stream-reader.
				                           	/// </summary>
				                           	/// <param name="file">The {{kindLowerCase}} file to retrieve the stream-reader for.</param>
				                           	/// <returns>The stream-reader to access the {{kindLowerCase}} file.</returns>
				                           	public static StreamReader GetReader(this {{kindString}}_{{pathAsClassName}} file)
				                           	{
				                           	    return new StreamReader(File.OpenRead({{getItemPathMethodName}}(file))!, leaveOpen:false);
				                           	}
				                           	
				                           	/// <summary>
				                           	/// Reads the {{kindLowerCase}} file's bytes asynchronously.
				                           	/// </summary>
				                           	/// <param name="file">The {{kindLowerCase}} file to read all bytes.</param>
				                           	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
				                           	/// <returns>bytes.</returns>
				                           	public static async Task<byte[]> ReadAllBytesAsync(this {{kindString}}_{{pathAsClassName}} file, CancellationToken cancellationToken = default(CancellationToken))
				                           	{
				                           	    return await File.ReadAllBytesAsync({{getItemPathMethodName}}(file), cancellationToken)!;
				                           	}
				                           	
				                           	/// <summary>
				                           	/// Reads the {{kindLowerCase}} file's bytes.
				                           	/// </summary>
				                           	/// <param name="file">The {{kindLowerCase}} file to read all bytes.</param>
				                           	/// <returns>bytes.</returns>
				                           	public static byte[] ReadAllBytes(this {{kindString}}_{{pathAsClassName}} file)
				                           	{
				                           	    return File.ReadAllBytes({{getItemPathMethodName}}(file))!;
				                           	}
				                           	
				                           	/// <summary>
				                           	/// Reads the {{kindLowerCase}} file's text asynchronously.
				                           	/// </summary>
				                           	/// <param name="file">The {{kindLowerCase}} file to read all text.</param>
				                           	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
				                           	/// <returns>text.</returns>
				                           	public static async Task<string> ReadAllTextAsync(this {{kindString}}_{{pathAsClassName}} file, CancellationToken cancellationToken = default(CancellationToken))
				                           	{
				                           	    return await File.ReadAllTextAsync({{getItemPathMethodName}}(file), cancellationToken)!;
				                           	}
				                           	
				                           	/// <summary>
				                           	/// Reads the {{kindLowerCase}} file's text.
				                           	/// </summary>
				                           	/// <param name="file">The {{kindLowerCase}} file to read all text.</param>
				                           	/// <returns>text.</returns>
				                           	public static string ReadAllText(this {{kindString}}_{{pathAsClassName}} file)
				                           	{
				                           	    return File.ReadAllText({{getItemPathMethodName}}(file))!;
				                           	}
				                           	
				                           """);

				sourceBuilder.AppendLine($$"""
				                           
				                           	/// <summary>
				                           	/// Gets the {{kindLowerCase}} file's path.
				                           	/// </summary>
				                           	/// <param name="file">The {{kindLowerCase}} file to retrieve the name for.</param>
				                           	/// <returns>The name to access the {{kindLowerCase}} file.</returns>
				                           	public static string {{getItemPathMethodName}}(this {{kindString}}_{{pathAsClassName}} file)
				                           	{
				                           	    return file switch 
				                           	    {
				                           """);

				foreach ((string relativePath, string identifierName, string resourceName, _) in pathGrouped)
				{
					string nonPathedIdentifierName = Utils.GetValidIdentifierName(Path.GetFileName(relativePath));

					sourceBuilder.AppendLine($$"""
					                           	        {{kindString}}_{{pathAsClassName}}.{{nonPathedIdentifierName}} => @"{{relativePath}}",
					                           """);
				}

				sourceBuilder.AppendLine("""            _ => throw new InvalidOperationException(),""");

				sourceBuilder.AppendLine("        };");

				sourceBuilder.AppendLine("    }");
			}
		}

		sourceBuilder.AppendLine("}");

		sourceBuilder.AppendLine($$"""

		                           /// <summary>
		                           /// Auto-generated enumeration for all {{kindLowerCase}} files in the assembly.
		                           /// </summary>
		                           public enum {{kindString}}
		                           {
		                           """);

		foreach ((string _, string identifierName, string resourceName, _) in contentFiles)
		{
			sourceBuilder.AppendLine($$"""
			                           	/// <summary>
			                           	/// Represents the {{kindLowerCase}} file '{{resourceName}}'.
			                           	/// </summary>
			                           	{{identifierName}},
			                           """);
		}

		sourceBuilder.AppendLine("}");

		foreach (IGrouping<string, ResourceItem> pathGrouped in contentFiles.GroupBy(g =>
					 Path.GetDirectoryName(g.RelativePath)))
		{
			string pathAsClassName = Utils.PathAsClassname(pathGrouped.Key, "_");
			if (!string.IsNullOrEmpty(pathGrouped.Key))
			{
				sourceBuilder.AppendLine($$"""

				                           /// <summary>
				                           /// Auto-generated enumeration for all {{kindLowerCase}} files in '{{pathGrouped.Key}}'.
				                           /// </summary>
				                           public enum {{kindString}}_{{pathAsClassName}}
				                           {
				                           """);

				foreach (ResourceItem item in pathGrouped)
				{
					string nonPathedIdentifierName = Utils.GetValidIdentifierName(Path.GetFileName(item.RelativePath));

					sourceBuilder.AppendLine($"""
					                          	/// <summary>
					                          	/// Represents the {kindLowerCase} file '{Path.GetFileName(item.RelativePath)}' in {pathGrouped.Key}.
					                          	/// </summary>
					                          	{nonPathedIdentifierName},
					                          """);
				}

				sourceBuilder.AppendLine("}");
			}
		}

		sourceBuilder.Append("#nullable restore");

		SourceText source = SourceText.From(sourceBuilder.ToString(), Encoding.UTF8);
		context.AddSource($"{kindString}s.generated.cs", source);
	}
}