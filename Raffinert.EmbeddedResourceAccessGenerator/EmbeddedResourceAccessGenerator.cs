namespace Raffinert.EmbeddedResourceAccessGenerator;

using Microsoft.CodeAnalysis;
using Raffinert.AccessGenerator.Core;

/// <summary>
/// The generator for the embedded resource access.
/// </summary>
[Generator]
public class EmbeddedResourceAccessGenerator : IIncrementalGenerator
{
	private static readonly DiagnosticDescriptor generationWarning = new DiagnosticDescriptor(
		id: "REMBRESGEN001",
		title: "Exception on generation",
		messageFormat: "Exception '{0}' {1}",
		category: "Raffinert.EmbeddedResourceAccessGenerator",
		DiagnosticSeverity.Error,
		isEnabledByDefault: true);

#if DEBUG
	private static readonly DiagnosticDescriptor logInfo = new DiagnosticDescriptor(
		id: "REMBRESGENLOG",
		title: "Log",
		messageFormat: "{0}",
		category: "Raffinert.EmbeddedResourceAccessGenerator",
		DiagnosticSeverity.Warning,
		isEnabledByDefault: true);
#endif

	/// <inheritdoc />
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		//Debugger.Launch();

		var combined = GeneratorHelper.GetConfiguredProvider(context, ResourceKind.EmbeddedResource);
		context.RegisterSourceOutput(combined, EmbeddedResourceAccessGenerator.GenerateSourceIncremental);
	}

	private static void GenerateSourceIncremental(SourceProductionContext context, GenerationContext generationContext)
	{
		try
		{
			EmbeddedResourceAccessGenerator.GenerateSource(context, generationContext);
		}
		catch (Exception e)
		{
			// We generate a diagnostic message on all internal failures.
			context.ReportDiagnostic(Diagnostic.Create(EmbeddedResourceAccessGenerator.generationWarning, Location.None,
				e.Message, e.StackTrace));
		}
	}


	private static void GenerateSource(SourceProductionContext context, GenerationContext generationContext)
	{
		if (generationContext.IsEmpty || string.IsNullOrWhiteSpace(generationContext.RootNamespace))
		{
			return;
		}

		Raffinert.AccessGenerator.Core.EmbeddedResourceAccessGenerator.GenerateCode(context, generationContext);
	}

	private void Log(SourceProductionContext context, string log)
	{
#if DEBUG
		context.ReportDiagnostic(Diagnostic.Create(EmbeddedResourceAccessGenerator.logInfo, Location.None, log));
#endif
	}
}