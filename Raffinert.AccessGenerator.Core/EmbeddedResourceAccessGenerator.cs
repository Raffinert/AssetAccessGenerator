namespace Raffinert.AccessGenerator.Core;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

public static class EmbeddedResourceAccessGenerator
{
	public static void GenerateCode(SourceProductionContext context, GenerationContext generationContext)
	{
		var embeddedResources = generationContext.With(ResourceKind.EmbeddedResource);

		if (embeddedResources.IsEmpty)
		{
			return;
		}

		StringBuilder sourceBuilder = new();
		sourceBuilder.AppendLine($"""
		                            #nullable enable
		                            namespace {embeddedResources.RootNamespace};
		                            using System;
		                            using System.IO;
		                            using System.Reflection;
		                            using System.Threading;
		                            using System.Threading.Tasks;
		                            """);

		if (embeddedResources.IsXunitDataAttributeAvailable)
		{

			sourceBuilder.AppendLine("""
			                         using System.Reflection;
			                         using Xunit.Sdk;
			                         """);

		}

		sourceBuilder.AppendLine("""
		                         /// <summary>
		                         /// Auto-generated class to access all embedded resources in an assembly.
		                         /// </summary>
		                         public static partial class EmbeddedResources
		                         {
		                             /// <summary>
		                             /// Retrieves a collection of embedded resources that match the specified pattern literal.
		                             /// </summary>
		                             /// <param name="pattern">The search pattern literal to match embedded resources.</param>
		                             /// <returns>
		                             /// An <see cref="IEnumerable{EmbeddedResource}"/> containing the matched embedded resources.
		                             /// </returns>
		                             public static IEnumerable<EmbeddedResource> GetMatches(string pattern)
		                             {
		                         """);

		var parsedGlobs = embeddedResources.MatchesLiterals
			.Distinct()
			.Select(ParsedGlob.Create)
			.Where(x => x.IsCorrect)
			.ToArray();

		var matchedFiles = parsedGlobs
			.Select(g => (g.Pattern, Matches: embeddedResources.Where(c => g.Glob.IsMatch(c.RelativePath)).ToArray()))
			.Where(x => x.Matches.Length > 0)
			.OrderBy(x => x.Pattern)
			.ToArray();

		if (matchedFiles.Length == 0)
		{
			sourceBuilder.AppendLine("""
			                                  return Enumerable.Empty<EmbeddedResource>();
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
					sourceBuilder.AppendLine($"                yield return EmbeddedResource.{Utils.PathAsClassname(match, "_")};");
				}

				sourceBuilder.AppendLine("                break;");
			}

			sourceBuilder.AppendLine("""
			                                 }
			                             }
			                             
			                         """);
		}

		if (embeddedResources.IsXunitDataAttributeAvailable)
		{

			sourceBuilder.AppendLine("""
			                             /// <summary>
			                             /// An xUnit attribute designed for use with the [Theory] attribute, which returns a collection of embedded resources matching the specified pattern literal.
			                             /// </summary>
			                             /// <param name="pattern">The search pattern literal to match embedded resources.</param>
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

		sourceBuilder.AppendLine("""
		                         	/// <summary>
		                         	/// Gets the embedded resource's stream.
		                         	/// </summary>
		                         	/// <param name="resource">The embedded resource to retrieve the stream for.</param>
		                         	/// <returns>The stream to access the embedded resource.</returns>
		                         	public static Stream GetStream(this EmbeddedResource resource)
		                         	{
		                         	    Assembly assembly = typeof(EmbeddedResources).Assembly;
		                         	    return assembly.GetManifestResourceStream(GetResourceName(resource))!;
		                         	}
		                         
		                         	/// <summary>
		                         	/// Gets the embedded resource's stream-reader.
		                         	/// </summary>
		                         	/// <param name="resource">The embedded resource to retrieve the stream-reader for.</param>
		                         	/// <returns>The stream-reader to access the embedded resource.</returns>
		                         	public static StreamReader GetReader(this EmbeddedResource resource)
		                         	{
		                         	    Assembly assembly = typeof(EmbeddedResources).Assembly;
		                         	    return new StreamReader(assembly.GetManifestResourceStream(GetResourceName(resource))!, leaveOpen:false);
		                         	}

		                         	/// <summary>
		                         	/// Reads the embedded resource's bytes asynchronously.
		                         	/// </summary>
		                         	/// <param name="resource">The embedded resource to read all bytes.</param>
		                         	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		                         	/// <returns>bytes.</returns>
		                         	public static async Task<byte[]> ReadAllBytesAsync(this EmbeddedResource resource, CancellationToken cancellationToken = default(CancellationToken))
		                         	{
		                         	    await using var resourceStream = resource.GetStream();
		                         	    using var memoryStream = new MemoryStream();
		                         	    await resourceStream.CopyToAsync(memoryStream);
		                         	    return memoryStream.ToArray();
		                         	}
		                         	
		                         	/// <summary>
		                         	/// Reads the embedded resource's bytes.
		                         	/// </summary>
		                         	/// <param name="resource">The embedded resource to read all bytes.</param>
		                         	/// <returns>bytes.</returns>
		                         	public static byte[] ReadAllBytes(this EmbeddedResource resource)
		                         	{
		                         	    using var resourceStream = resource.GetStream();
		                         	    using var memoryStream = new MemoryStream();
		                         	    resourceStream.CopyTo(memoryStream);
		                         	    return memoryStream.ToArray();
		                         	}
		                         	
		                         	/// <summary>
		                         	/// Reads the embedded resource's text asynchronously.
		                         	/// </summary>
		                         	/// <param name="resource">The embedded resource to read all text.</param>
		                         	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		                         	/// <returns>text.</returns>
		                         	public static async Task<string> ReadAllTextAsync(this EmbeddedResource resource, CancellationToken cancellationToken = default(CancellationToken))
		                         	{
		                         	    using StreamReader reader = resource.GetReader();
		                         	    return await reader.ReadToEndAsync()!;
		                         	}
		                         	
		                         	/// <summary>
		                         	/// Reads the embedded resource's text.
		                         	/// </summary>
		                         	/// <param name="resource">The embedded resource to read all text.</param>
		                         	/// <returns>text.</returns>
		                         	public static string ReadAllText(this EmbeddedResource resource)
		                         	{
		                         	    using StreamReader reader = resource.GetReader();
		                         	    return reader.ReadToEnd()!;
		                         	}
		                         	
		                         """);

		sourceBuilder.AppendLine("""
		                         	/// <summary>
		                         	/// Gets the embedded resource's name in the format required by <c>GetManifestResourceStream</c>.
		                         	/// </summary>
		                         	/// <param name="resource">The embedded resource to retrieve the name for.</param>
		                         	/// <returns>The name to access the embedded resource.</returns>
		                         	public static string GetResourceName(this EmbeddedResource resource)
		                         	{
		                         	    return resource switch 
		                         	    {
		                         """);

		foreach ((string _, string identifierName, string resourceName, _) in embeddedResources)
		{
			sourceBuilder.AppendLine($$"""
			                           	        EmbeddedResource.{{identifierName}} => "{{embeddedResources.RootNamespace}}.{{resourceName}}",
			                           """);
		}

		sourceBuilder.AppendLine("""            _ => throw new InvalidOperationException(),""");

		sourceBuilder.AppendLine("        };");

		sourceBuilder.AppendLine("    }");

		foreach (IGrouping<string, ResourceItem> pathGrouped in embeddedResources.GroupBy(g =>
					 Path.GetDirectoryName(g.RelativePath)))
		{
			string pathAsClassName = Utils.PathAsClassname(pathGrouped.Key, "_");
			if (!string.IsNullOrEmpty(pathGrouped.Key))
			{
				sourceBuilder.AppendLine($$"""
				                           
				                           	/// <summary>
				                           	/// Gets the embedded resource's stream.
				                           	/// </summary>
				                           	/// <param name="resource">The embedded resource to retrieve the stream for.</param>
				                           	/// <returns>The stream to access the embedded resource.</returns>
				                           	public static Stream GetStream(this EmbeddedResource_{{pathAsClassName}} resource)
				                           	{
				                           	    Assembly assembly = typeof(EmbeddedResources).Assembly;
				                           	    return assembly.GetManifestResourceStream(GetResourceName(resource))!;
				                           	}
				                           
				                           	/// <summary>
				                           	/// Gets the embedded resource's stream-reader.
				                           	/// </summary>
				                           	/// <param name="resource">The embedded resource to retrieve the stream-reader for.</param>
				                           	/// <returns>The stream-reader to access the embedded resource.</returns>
				                           	public static StreamReader GetReader(this EmbeddedResource_{{pathAsClassName}} resource)
				                           	{
				                           	    Assembly assembly = typeof(EmbeddedResources).Assembly;
				                           	    return new StreamReader(assembly.GetManifestResourceStream(GetResourceName(resource))!, leaveOpen:false);
				                           	}
				                           	
				                           	/// <summary>
				                           	/// Reads the embedded resource's bytes asynchronously.
				                           	/// </summary>
				                           	/// <param name="resource">The embedded resource to read all bytes.</param>
				                           	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
				                           	/// <returns>bytes.</returns>
				                           	public static async Task<byte[]> ReadAllBytesAsync(this EmbeddedResource_{{pathAsClassName}} resource, CancellationToken cancellationToken = default(CancellationToken))
				                           	{
				                           	    await using var resourceStream = resource.GetStream();
				                           	    using var memoryStream = new MemoryStream();
				                           	    await resourceStream.CopyToAsync(memoryStream);
				                           	    return memoryStream.ToArray();
				                           	}
				                           	
				                           	/// <summary>
				                           	/// Reads the embedded resource's bytes.
				                           	/// </summary>
				                           	/// <param name="resource">The embedded resource to read all bytes.</param>
				                           	/// <returns>bytes.</returns>
				                           	public static byte[] ReadAllBytes(this EmbeddedResource_{{pathAsClassName}} resource)
				                           	{
				                           	    using var resourceStream = resource.GetStream();
				                           	    using var memoryStream = new MemoryStream();
				                           	    resourceStream.CopyTo(memoryStream);
				                           	    return memoryStream.ToArray();
				                           	}
				                           	
				                           	/// <summary>
				                           	/// Reads the embedded resource's text asynchronously.
				                           	/// </summary>
				                           	/// <param name="resource">The embedded resource to read all text.</param>
				                           	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
				                           	/// <returns>text.</returns>
				                           	public static async Task<string> ReadAllTextAsync(this EmbeddedResource_{{pathAsClassName}} resource, CancellationToken cancellationToken = default(CancellationToken))
				                           	{
				                           	    using StreamReader reader = resource.GetReader();
				                           	    return await reader.ReadToEndAsync()!;
				                           	}
				                           	
				                           	/// <summary>
				                           	/// Reads the embedded resource's text.
				                           	/// </summary>
				                           	/// <param name="resource">The embedded resource to read all text.</param>
				                           	/// <returns>text.</returns>
				                           	public static string ReadAllText(this EmbeddedResource_{{pathAsClassName}} resource)
				                           	{
				                           	    using StreamReader reader = resource.GetReader();
				                           	    return reader.ReadToEnd()!;
				                           	}
				                           	
				                           """);

				sourceBuilder.AppendLine($$"""
				                           
				                           	/// <summary>
				                           	/// Gets the embedded resource's name in the format required by <c>GetManifestResourceStream</c>.
				                           	/// </summary>
				                           	/// <param name="resource">The embedded resource to retrieve the name for.</param>
				                           	/// <returns>The name to access the embedded resource.</returns>
				                           	public static string GetResourceName(this EmbeddedResource_{{pathAsClassName}} resource)
				                           	{
				                           	    return resource switch 
				                           	    {
				                           """);

				foreach ((string relativePath, string identifierName, string resourceName, _) in pathGrouped)
				{
					string nonPathedIdentifierName = Utils.GetValidIdentifierName(Path.GetFileName(relativePath));

					sourceBuilder.AppendLine($$"""
					                           	        EmbeddedResource_{{pathAsClassName}}.{{nonPathedIdentifierName}} => "{{embeddedResources.RootNamespace}}.{{resourceName}}",
					                           """);
				}

				sourceBuilder.AppendLine("""            _ => throw new InvalidOperationException(),""");

				sourceBuilder.AppendLine("        };");

				sourceBuilder.AppendLine("    }");
			}
		}

		sourceBuilder.AppendLine("}");

		sourceBuilder.AppendLine("""

		                         /// <summary>
		                         /// Auto-generated enumeration for all embedded resources in the assembly.
		                         /// </summary>
		                         public enum EmbeddedResource
		                         {
		                         """);

		foreach ((string _, string identifierName, string resourceName, _) in embeddedResources)
		{
			sourceBuilder.AppendLine($$"""
			                           	/// <summary>
			                           	/// Represents the embedded resource '{{resourceName}}'.
			                           	/// </summary>
			                           	{{identifierName}},
			                           """);
		}

		sourceBuilder.AppendLine("}");

		foreach (IGrouping<string, ResourceItem> pathGrouped in embeddedResources.GroupBy(g =>
					 Path.GetDirectoryName(g.RelativePath)))
		{
			string pathAsClassName = Utils.PathAsClassname(pathGrouped.Key, "_");
			if (!string.IsNullOrEmpty(pathGrouped.Key))
			{
				sourceBuilder.AppendLine($$"""

				                           /// <summary>
				                           /// Auto-generated enumeration for all embedded resources in '{{pathGrouped.Key}}'.
				                           /// </summary>
				                           public enum EmbeddedResource_{{pathAsClassName}}
				                           {
				                           """);

				foreach (ResourceItem item in pathGrouped)
				{
					string nonPathedIdentifierName = Utils.GetValidIdentifierName(Path.GetFileName(item.RelativePath));

					sourceBuilder.AppendLine($$"""
					                           	/// <summary>
					                           	/// Represents the embedded resource '{{Path.GetFileName(item.RelativePath)}}' in {{pathGrouped.Key}}.
					                           	/// </summary>
					                           	{{nonPathedIdentifierName}},
					                           """);
				}

				sourceBuilder.AppendLine("}");
			}
		}

		sourceBuilder.Append("#nullable restore");

		SourceText source = SourceText.From(sourceBuilder.ToString(), Encoding.UTF8);
		context.AddSource("EmbeddedResources.generated.cs", source);
	}
}