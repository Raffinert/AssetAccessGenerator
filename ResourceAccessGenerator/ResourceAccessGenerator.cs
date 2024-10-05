﻿namespace ResourceAccessGenerator;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

/// <summary>
/// The generator for the embedded and included resource access.
/// </summary>
[Generator]
public class ResourceAccessGenerator : IIncrementalGenerator
{
	private static readonly DiagnosticDescriptor generationWarning = new DiagnosticDescriptor(
		id: "RESGEN001",
		title: "Exception on generation",
		messageFormat: "Exception '{0}' {1}",
		category: "ResourceAccessGenerator",
		DiagnosticSeverity.Error,
		isEnabledByDefault: true);

#if DEBUG
	private static readonly DiagnosticDescriptor logInfo = new DiagnosticDescriptor(
		id: "RESGENLOG",
		title: "Log",
		messageFormat: "{0}",
		category: "ResourceAccessGenerator",
		DiagnosticSeverity.Warning,
		isEnabledByDefault: true);
#endif

	/// <inheritdoc />
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		//Debugger.Launch();

		// We need a value provider for any addition file.
		// As soon as there is direct access to embedded resources we can change this.
		// All embedded resources are added as additional files through our build props integrated into the nuget.
		IncrementalValueProvider<ImmutableArray<(string Path, ResourceKind Kind)>> additionaFilesProvider =
			context.AdditionalTextsProvider
				.Combine(context.AnalyzerConfigOptionsProvider) // Combine with options provider
				.Select((fileAndOptions, _) =>
				{
					var (file, optionsProvider) = fileAndOptions;
					// Get the options for the current file
					var options = optionsProvider.GetOptions(file);

					var included = options.TryGetValue("build_metadata.AdditionalFiles.GenerateIncludedResourceAccess",
									   out var generateIncludedStaticAccess)
								   && generateIncludedStaticAccess == "true";

					if (included)
					{
						return (file.Path, Kind: ResourceKind.Included);
					}

					var embedded = options.TryGetValue("build_metadata.EmbeddedResource.GenerateEmbeddedResourceAccess",
									   out var generateEmbeddedStaticAccess)
								   && generateEmbeddedStaticAccess == "true";

					if (embedded)
					{
						return (file.Path, Kind: ResourceKind.Embedded);
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

		// We combine the providers to generate the parameters for our source generation.
		IncrementalValueProvider<ResourceGenerationContext> combined = additionaFilesProvider
				.Combine(rootNamespaceProvider.Combine(buildProjectDirProvider)).Select((c, _) =>
					(c.Left, c.Right.Left, c.Right.Right))
				.Select(ResourceAccessGenerator.MapToResourceGenerationContext);

		context.RegisterSourceOutput(combined, ResourceAccessGenerator.GenerateSourceIncremental);
	}

	private static ResourceGenerationContext MapToResourceGenerationContext((ImmutableArray<(string Path, ResourceKind Kind)>, string?, string? Right) tuple, CancellationToken cancellationToken)
	{
		var (pathAndKinds, rootNamespace, buildProjectDir) = tuple;
		return new ResourceGenerationContext([
			..pathAndKinds.Select(pathAndKind =>
			{
				string resourcePath = Utils.GetRelativePath(pathAndKind.Path, buildProjectDir).Replace("%20", " ");
				string resourceName = Utils.GetResourceName(resourcePath);
				string identifierName = Utils.GetValidIdentifierName(resourcePath);
				return new ResourceItem(resourcePath, identifierName, resourceName, pathAndKind.Kind);
			})
		], rootNamespace);
	}

	private static void GenerateSourceIncremental(SourceProductionContext context, ResourceGenerationContext resourcesContext)
	{
		try
		{
			ResourceAccessGenerator.GenerateSource(context, resourcesContext);
		}
		catch (Exception e)
		{
			// We generate a diagnostic message on all internal failures.
			context.ReportDiagnostic(Diagnostic.Create(ResourceAccessGenerator.generationWarning, Location.None,
				e.Message, e.StackTrace));
		}
	}


	private static void GenerateSource(SourceProductionContext context, ResourceGenerationContext resourcesContext)
	{
		if (resourcesContext.IsEmpty)
		{
			return;
		}

		EmbeddedGenerator.GenerateCode(context, resourcesContext);
		IncludedGenerator.GenerateCode(context, resourcesContext);
	}


	private void Log(SourceProductionContext context, string log)
	{
#if DEBUG
		context.ReportDiagnostic(Diagnostic.Create(ResourceAccessGenerator.logInfo, Location.None, log));
#endif
	}
}