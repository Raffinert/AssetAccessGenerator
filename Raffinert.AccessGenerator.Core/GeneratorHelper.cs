namespace Raffinert.AccessGenerator.Core;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

public static class GeneratorHelper
{
	public static IncrementalValueProvider<GenerationContext> GetConfiguredProvider(IncrementalGeneratorInitializationContext context, ResourceKind resourceKind)
	{
		//Debugger.Launch();

		var typeName = $"{resourceKind}s";

		// We need a value provider for any addition file.
		// As soon as there is direct access to embedded resources we can change this.
		// All embedded resources are added as additional files through our build props integrated into the nuget.
		IncrementalValueProvider<ImmutableArray<(string Path, ResourceKind Kind)>> additionalFilesProvider =
			context.AdditionalTextsProvider
				.Combine(context.AnalyzerConfigOptionsProvider) // Combine with options provider
				.Select((fileAndOptions, _) =>
				{
					var (file, optionsProvider) = fileAndOptions;
					// Get the options for the current file
					var options = optionsProvider.GetOptions(file);

					var content = options.TryGetValue("build_metadata.Content.GenerateContentAccess",
									   out var generateContentAccess)
								   && generateContentAccess == "true";

					if (content)
					{
						return (file.Path, Kind: ResourceKind.Content);
						//return !file.Path.EndsWith("testhost.exe") && !file.Path.EndsWith("testhost.dll") 
						//	? (file.Path, Kind: ResourceKind.Content)
						//	: (file.Path, Kind: ResourceKind.Unspecified);
					}

					var none = options.TryGetValue("build_metadata.None.GenerateNoneAccess",
									  out var generateNoneAccess)
								  && generateNoneAccess == "true";

					if (none)
					{
						return (file.Path, Kind: ResourceKind.None);
					}

					var embedded = options.TryGetValue("build_metadata.EmbeddedResource.GenerateEmbeddedResourceAccess",
									   out var generateEmbeddedResourceAccess)
								   && generateEmbeddedResourceAccess == "true";

					if (embedded)
					{
						return (file.Path, Kind: ResourceKind.EmbeddedResource);
					}

					return (file.Path, Kind: ResourceKind.Unspecified);
				})
				.Where(filePathAndKind => filePathAndKind.Kind != ResourceKind.Unspecified)
				.Collect();

		// The root namespace value provider. Can this ever be null? So far I have not seen it.
		IncrementalValueProvider<string?> rootNamespaceProvider = context.AnalyzerConfigOptionsProvider.Select((x, _) =>
			x.GlobalOptions.TryGetValue("build_property.RootNamespace", out string? rootNamespace)
				? rootNamespace
				: null);

		// The project directory value provider. Can this ever be null? So far I have not seen it.
		IncrementalValueProvider<string?> buildProjectDirProvider = context.AnalyzerConfigOptionsProvider.Select(
			(x, _) =>
				x.GlobalOptions.TryGetValue("build_property.projectdir", out string? rootNamespace)
					? rootNamespace
					: null);

		// Extract globbing patterns from source code
		IncrementalValueProvider<ImmutableArray<string?>> globPatternsProvider =
			context.SyntaxProvider
				.CreateSyntaxProvider(
					predicate: (node, _) =>
					{
						if (node is not InvocationExpressionSyntax invocation) return false;
						if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) return false;
						if (memberAccess.Name.Identifier.Text != "GetMatches") return false;
						if (memberAccess.Expression is not IdentifierNameSyntax isx) return false;
						return isx.Identifier.Text == typeName;
					},
					transform: static (ctx, _) =>
					{
						var invocation = (InvocationExpressionSyntax)ctx.Node;

						if (invocation.ArgumentList.Arguments.Count == 1 &&
							invocation.ArgumentList.Arguments[0].Expression is LiteralExpressionSyntax literal &&
							literal.Kind() == SyntaxKind.StringLiteralExpression)
						{
							return literal.Token.ValueText;
						}

						return null;
					})
				.Where(pattern => pattern is not null)
				.Collect();

		// We combine the providers to generate the parameters for our source generation.
		IncrementalValueProvider<GenerationContext> combined = additionalFilesProvider
			.Combine(rootNamespaceProvider
				.Combine(buildProjectDirProvider)
				.Combine(globPatternsProvider)) // Adding globPatternsProvider here
			.Select((c, _) => (c.Left, c.Right.Left.Left, c.Right.Left.Right, c.Right.Right))
			.Select(GeneratorHelper.MapToResourceGenerationContext);

		return combined;
	}

	private static GenerationContext MapToResourceGenerationContext((ImmutableArray<(string Path, ResourceKind Kind)>, string, string, ImmutableArray<string>) valueTuple, CancellationToken cancellationToken)
	{
		var (pathAndKinds, rootNamespace, buildProjectDir, matchesLiterals) = valueTuple;

		if (buildProjectDir == null || rootNamespace == null)
		{
			return new GenerationContext(ImmutableArray<ResourceItem>.Empty, rootNamespace ?? "EmptyRootNamespace", ImmutableArray<string>.Empty);
		}

		return new GenerationContext(pathAndKinds.Select(pathAndKind =>
			{
				string resourcePath = Utils.GetRelativePath(pathAndKind.Path, buildProjectDir).Replace("%20", " ");
				string resourceName = Utils.GetResourceName(resourcePath);
				string identifierName = Utils.GetValidIdentifierName(resourcePath);
				// trick to skip testhost.exe and testhost.dll and other files that are external to the solution
				var kind = pathAndKind.Path.IsSubPathOf(buildProjectDir) ? pathAndKind.Kind : ResourceKind.Unspecified;
				return new ResourceItem(resourcePath, identifierName, resourceName, kind);
			}).ToImmutableArray(), rootNamespace, matchesLiterals);
	}
}