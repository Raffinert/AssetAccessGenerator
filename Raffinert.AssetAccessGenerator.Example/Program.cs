namespace Raffinert.AssetAccessGenerator.Example;

internal class Program
{
	private static void Main()
	{
		Program.Enum(EmbeddedResource.TestAsset_Test_txt);
		Program.Enum(EmbeddedResource.TestAsset_Test2_txt);
		Program.Enum(EmbeddedResource.RootTest_txt);
		Program.EnumExtension(EmbeddedResource.RootTest_txt);
		Program.EnumExtension(Content.RootTest___content_txt);
		Program.Enum(Content.RootTest___content_txt);

		var contentFiles = Contents.GetMatches("**/*").Concat(Contents.GetMatches("*"));
		foreach (var file in contentFiles)
		{
			Program.Enum(file);
		}

		List<IEnumerable<EmbeddedResource>> matches =
		[
			EmbeddedResources.GetMatches("**/*??/*"),
			EmbeddedResources.GetMatches("**/*e?/*"),
			EmbeddedResources.GetMatches("**/*?t/*"),
			EmbeddedResources.GetMatches("**/**")
		];

		var embeddedFiles = matches.SelectMany(x => x).Distinct();
		foreach (var file in embeddedFiles)
		{
			Program.Enum(file);
		}
	}

	private static void Enum(Content rootTestInclTxt)
	{
		Console.WriteLine(rootTestInclTxt.ReadAllText());
	}

	private static void EnumExtension(Content rootTestInclTxt)
	{
		Console.WriteLine(rootTestInclTxt.ReadAllText());
	}

	private static void Enum(EmbeddedResource resource)
	{
		using Stream s = resource.GetStream();
		using StreamReader sr = new StreamReader(s);
		Console.WriteLine(sr.ReadToEnd());
	}

	private static void EnumExtension(EmbeddedResource resource)
	{
		using Stream s = resource.GetStream();
		using StreamReader sr = new StreamReader(s);
		Console.WriteLine(sr.ReadToEnd());
	}
}