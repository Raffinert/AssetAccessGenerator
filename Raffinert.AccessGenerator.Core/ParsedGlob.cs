namespace Raffinert.AccessGenerator.Core;

using DotNet.Globbing;

internal class ParsedGlob
{
	private ParsedGlob(string pattern)
	{
		this.Pattern = pattern;
	}

	public string Pattern { get; private set; }
	public bool IsCorrect { get; private set; }
	public Glob Glob { get; private set; }

	public static ParsedGlob Create(string pattern)
	{
		var instance = new ParsedGlob(pattern: pattern);
		try
		{
			instance.Glob = Glob.Parse(pattern);
			instance.IsCorrect = true;
		}
		catch
		{
		}

		return instance;
	}
}