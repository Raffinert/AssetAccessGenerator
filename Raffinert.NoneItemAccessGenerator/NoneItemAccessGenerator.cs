﻿namespace Raffinert.NoneItemAccessGenerator;

using Microsoft.CodeAnalysis;
using Raffinert.AccessGenerator.Core;

/// <summary>
/// The generator for the embedded and included resource access.
/// </summary>
[Generator]
public class NoneItemAccessGenerator : IIncrementalGenerator
{
	private static readonly DiagnosticDescriptor generationWarning = new DiagnosticDescriptor(
		id: "RNONEITMGEN001",
		title: "Exception on generation",
		messageFormat: "Exception '{0}' {1}",
		category: "Raffinert.NoneItemAccessGenerator",
		DiagnosticSeverity.Error,
		isEnabledByDefault: true);

#if DEBUG
	private static readonly DiagnosticDescriptor logInfo = new DiagnosticDescriptor(
		id: "RNONEITMGENLOG",
		title: "Log",
		messageFormat: "{0}",
		category: "Raffinert.NoneItemAccessGenerator",
		DiagnosticSeverity.Warning,
		isEnabledByDefault: true);
#endif

	/// <inheritdoc />
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		//Debugger.Launch();

		var combined = GeneratorHelper.GetConfiguredProvider(context, ResourceKind.None);
		context.RegisterSourceOutput(combined, NoneItemAccessGenerator.GenerateSourceIncremental);
	}

	private static void GenerateSourceIncremental(SourceProductionContext context, GenerationContext generationContext)
	{
		try
		{
			NoneItemAccessGenerator.GenerateSource(context, generationContext);
		}
		catch (Exception e)
		{
			// We generate a diagnostic message on all internal failures.
			context.ReportDiagnostic(Diagnostic.Create(NoneItemAccessGenerator.generationWarning, Location.None,
				e.Message, e.StackTrace));
		}
	}


	private static void GenerateSource(SourceProductionContext context, GenerationContext generationContext)
	{
		if (generationContext.IsEmpty || string.IsNullOrWhiteSpace(generationContext.RootNamespace))
		{
			return;
		}

		AccessGenerator.Core.FileAccessGenerator.GenerateCode(context, generationContext, ResourceKind.None);
	}

	private void Log(SourceProductionContext context, string log)
	{
#if DEBUG
		context.ReportDiagnostic(Diagnostic.Create(NoneItemAccessGenerator.logInfo, Location.None, log));
#endif
	}
}