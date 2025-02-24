﻿namespace Raffinert.ContentItemAccessGenerator;

using Microsoft.CodeAnalysis;
using Raffinert.AccessGenerator.Core;

/// <summary>
/// The generator for the content item access.
/// </summary>
[Generator]
public class ContentItemAccessGenerator : IIncrementalGenerator
{
	private static readonly DiagnosticDescriptor generationWarning = new DiagnosticDescriptor(
		id: "RCONTITMGEN001",
		title: "Exception on generation",
		messageFormat: "Exception '{0}' {1}",
		category: "Raffinert.ContentItemAccessGenerator",
		DiagnosticSeverity.Error,
		isEnabledByDefault: true);

#if DEBUG
	private static readonly DiagnosticDescriptor logInfo = new DiagnosticDescriptor(
		id: "RCONTITMGENLOG",
		title: "Log",
		messageFormat: "{0}",
		category: "Raffinert.ContentItemAccessGenerator",
		DiagnosticSeverity.Warning,
		isEnabledByDefault: true);
#endif

	/// <inheritdoc />
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		//Debugger.Launch();

		var combined = GeneratorHelper.GetConfiguredProvider(context, ResourceKind.Content);
		context.RegisterSourceOutput(combined, ContentItemAccessGenerator.GenerateSourceIncremental);
	}

	private static void GenerateSourceIncremental(SourceProductionContext context, GenerationContext generationContext)
	{
		try
		{
			ContentItemAccessGenerator.GenerateSource(context, generationContext);
		}
		catch (Exception e)
		{
			// We generate a diagnostic message on all internal failures.
			context.ReportDiagnostic(Diagnostic.Create(ContentItemAccessGenerator.generationWarning, Location.None,
				e.Message, e.StackTrace));
		}
	}


	private static void GenerateSource(SourceProductionContext context, GenerationContext generationContext)
	{
		if (generationContext.IsEmpty || string.IsNullOrWhiteSpace(generationContext.RootNamespace))
		{
			return;
		}

		AccessGenerator.Core.FileAccessGenerator.GenerateCode(context, generationContext, ResourceKind.Content);
	}

	private void Log(SourceProductionContext context, string log)
	{
#if DEBUG
		context.ReportDiagnostic(Diagnostic.Create(ContentItemAccessGenerator.logInfo, Location.None, log));
#endif
	}
}