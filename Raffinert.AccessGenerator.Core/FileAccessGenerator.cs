﻿namespace Raffinert.AccessGenerator.Core;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

public static class FileAccessGenerator
{
	public static void GenerateCode(SourceProductionContext context, GenerationContext generationContext, ResourceKind kind)
	{
		var contentFiles = generationContext.With(kind);

		if (contentFiles.IsEmpty)
		{
			return;
		}

		var kindString = kind.ToString();
		var kindLowerCase = kindString.ToLowerInvariant();
		var getItemPathMethodName = $"Get{kind}FilePath";

		StringBuilder sourceBuilder = new();
		sourceBuilder.AppendLine($$"""
		                           #nullable enable
		                           namespace {{contentFiles.RootNamespace}};
		                           using System;
		                           using System.IO;
		                           using System.Threading;
		                           using System.Threading.Tasks;

		                           /// <summary>
		                           /// Auto-generated class to access all {{kindLowerCase}} files in an assembly.
		                           /// </summary>
		                           public static partial class {{kindString}}s
		                           {
		                           """);

		sourceBuilder.AppendLine($$"""
		                           	/// <summary>
		                           	/// Gets the {{kindLowerCase}} file's stream.
		                           	/// </summary>
		                           	/// <param name="file">The {{kindLowerCase}} file to retrieve the stream for.</param>
		                           	/// <returns>The stream to access the {{kindLowerCase}} file.</returns>
		                           	public static Stream GetStream(this {{kind}} file)
		                           	{
		                           		return File.OpenRead({{getItemPathMethodName}}(file))!;
		                           	}
		                           
		                           	/// <summary>
		                           	/// Gets the {{kindLowerCase}} file's stream-reader.
		                           	/// </summary>
		                           	/// <param name="file">The {{kindLowerCase}} file to retrieve the stream-reader for.</param>
		                           	/// <returns>The stream-reader to access the {{kindLowerCase}} file.</returns>
		                           	public static StreamReader GetReader(this {{kind}} file)
		                           	{
		                           		return new StreamReader(File.OpenRead({{getItemPathMethodName}}(file))!, leaveOpen:false);
		                           	}
		                           	
		                           	/// <summary>
		                           	/// Reads the {{kindLowerCase}} file's bytes asynchronously.
		                           	/// </summary>
		                           	/// <param name="file">The {{kindLowerCase}} file to read all bytes.</param>
		                           	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		                           	/// <returns>bytes.</returns>
		                           	public static async Task<byte[]> ReadAllBytesAsync(this {{kind}} file, CancellationToken cancellationToken = default(CancellationToken))
		                           	{
		                           	    return await File.ReadAllBytesAsync({{getItemPathMethodName}}(file), cancellationToken)!;
		                           	}
		                           	
		                           	/// <summary>
		                           	/// Reads the {{kindLowerCase}} file's bytes.
		                           	/// </summary>
		                           	/// <param name="file">The {{kindLowerCase}} file to read all bytes.</param>
		                           	/// <returns>bytes.</returns>
		                           	public static byte[] ReadAllBytes(this {{kind}} file)
		                           	{
		                           	    return File.ReadAllBytes({{getItemPathMethodName}}(file))!;
		                           	}
		                           	
		                           	/// <summary>
		                           	/// Reads the {{kindLowerCase}} file's text asynchronously.
		                           	/// </summary>
		                           	/// <param name="file">The {{kindLowerCase}} file to read all text.</param>
		                           	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		                           	/// <returns>text.</returns>
		                           	public static async Task<string> ReadAllTextAsync(this {{kind}} file, CancellationToken cancellationToken = default(CancellationToken))
		                           	{
		                           	    return await File.ReadAllTextAsync({{getItemPathMethodName}}(file), cancellationToken)!;
		                           	}
		                           	
		                           	/// <summary>
		                           	/// Reads the {{kindLowerCase}} file's text.
		                           	/// </summary>
		                           	/// <param name="file">The {{kindLowerCase}} file to read all text.</param>
		                           	/// <returns>text.</returns>
		                           	public static string ReadAllText(this {{kind}} file)
		                           	{
		                           	    return File.ReadAllText({{getItemPathMethodName}}(file))!;
		                           	}
		                           	
		                           	/// <summary>
		                            /// Gets the {{kindLowerCase}} file's FileInfo.
		                            /// </summary>
		                            /// <param name="file">The {{kindLowerCase}} file to retrieve the FileInfo for.</param>
		                            /// <returns>The FileInfo to access the {{kindLowerCase}} file.</returns>
		                            public static FileInfo GetFileInfo(this {{kind}} file)
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
		                         	public static string {{getItemPathMethodName}}(this {{kind}} file)
		                         	{
		                         		return file switch 
		                         		{
		                         """);

		foreach ((string path, string identifierName, string _, _) in contentFiles)
		{
			sourceBuilder.AppendLine($$"""
			                           			{{kind}}.{{identifierName}} => @"{{path}}",
			                           """);
		}

		sourceBuilder.AppendLine("""			_ => throw new InvalidOperationException(),""");

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
				                           	public static Stream GetStream(this {{kind}}_{{pathAsClassName}} file)
				                           	{
				                           		return File.OpenRead({{getItemPathMethodName}}(file))!;
				                           	}
				                           
				                           	/// <summary>
				                           	/// Gets the {{kindLowerCase}} file's stream-reader.
				                           	/// </summary>
				                           	/// <param name="file">The {{kindLowerCase}} file to retrieve the stream-reader for.</param>
				                           	/// <returns>The stream-reader to access the {{kindLowerCase}} file.</returns>
				                           	public static StreamReader GetReader(this {{kind}}_{{pathAsClassName}} file)
				                           	{
				                           		return new StreamReader(File.OpenRead({{getItemPathMethodName}}(file))!, leaveOpen:false);
				                           	}
				                           	
				                           	/// <summary>
				                           	/// Reads the {{kindLowerCase}} file's bytes asynchronously.
				                           	/// </summary>
				                           	/// <param name="file">The {{kindLowerCase}} file to read all bytes.</param>
				                           	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
				                           	/// <returns>bytes.</returns>
				                           	public static async Task<byte[]> ReadAllBytesAsync(this {{kind}}_{{pathAsClassName}} file, CancellationToken cancellationToken = default(CancellationToken))
				                           	{
				                           	    return await File.ReadAllBytesAsync({{getItemPathMethodName}}(file), cancellationToken)!;
				                           	}
				                           	
				                           	/// <summary>
				                           	/// Reads the {{kindLowerCase}} file's bytes.
				                           	/// </summary>
				                           	/// <param name="file">The {{kindLowerCase}} file to read all bytes.</param>
				                           	/// <returns>bytes.</returns>
				                           	public static byte[] ReadAllBytes(this {{kind}}_{{pathAsClassName}} file)
				                           	{
				                           	    return File.ReadAllBytes({{getItemPathMethodName}}(file))!;
				                           	}
				                           	
				                           	/// <summary>
				                           	/// Reads the {{kindLowerCase}} file's text asynchronously.
				                           	/// </summary>
				                           	/// <param name="file">The {{kindLowerCase}} file to read all text.</param>
				                           	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
				                           	/// <returns>text.</returns>
				                           	public static async Task<string> ReadAllTextAsync(this {{kind}}_{{pathAsClassName}} file, CancellationToken cancellationToken = default(CancellationToken))
				                           	{
				                           	    return await File.ReadAllTextAsync({{getItemPathMethodName}}(file), cancellationToken)!;
				                           	}
				                           	
				                           	/// <summary>
				                           	/// Reads the {{kindLowerCase}} file's text.
				                           	/// </summary>
				                           	/// <param name="file">The {{kindLowerCase}} file to read all text.</param>
				                           	/// <returns>text.</returns>
				                           	public static string ReadAllText(this {{kind}}_{{pathAsClassName}} file)
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
				                           	public static string {{getItemPathMethodName}}(this {{kind}}_{{pathAsClassName}} file)
				                           	{
				                           		return file switch 
				                           		{
				                           """);

				foreach ((string relativePath, string identifierName, string resourceName, _) in pathGrouped)
				{
					string nonPathedIdentifierName = Utils.GetValidIdentifierName(Path.GetFileName(relativePath));

					sourceBuilder.AppendLine($$"""
					                           			{{kind}}_{{pathAsClassName}}.{{nonPathedIdentifierName}} => @"{{relativePath}}",
					                           """);
				}

				sourceBuilder.AppendLine("""			_ => throw new InvalidOperationException(),""");

				sourceBuilder.AppendLine("        };");

				sourceBuilder.AppendLine("    }");
			}
		}

		sourceBuilder.AppendLine("}");

		sourceBuilder.AppendLine($$"""

		                         /// <summary>
		                         /// Auto-generated enumeration for all {{kindLowerCase}} files in the assembly.
		                         /// </summary>
		                         public enum {{kind}}
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
				                           public enum {{kind}}_{{pathAsClassName}}
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
		context.AddSource($"{kind}s.generated.cs", source);
	}
}