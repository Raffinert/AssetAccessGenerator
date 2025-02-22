namespace Raffinert.AssetAccessGenerator.Tests.Tests;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

public class NoneFilesTests(ITestOutputHelper testOutputHelper)
{
	[Fact]
	public async Task NoneItemTestTxtIsAccessible()
	{
		using var reader = None.TestNoneAssets_NoneTest_txt.GetReader();
		Assert.Equal("Success", await reader.ReadToEndAsync());

		var text = None.TestNoneAssets_NoneTest_txt.ReadAllText();
		Assert.Equal("Success", text);

		var text2 = await None.TestNoneAssets_NoneTest_txt.ReadAllTextAsync(CancellationToken.None);
		Assert.Equal("Success", text2);

		var bytes1 = await None.TestNoneAssets_NoneTest_txt.ReadAllBytesAsync(CancellationToken.None);

		Assert.Equivalent(new byte[] { 239, 187, 191, 83, 117, 99, 99, 101, 115, 115 }, bytes1);

		var bytes2 = None.TestNoneAssets_NoneTest_txt.ReadAllBytes();

		Assert.Equivalent(new byte[] { 239, 187, 191, 83, 117, 99, 99, 101, 115, 115 }, bytes2);

		var file = None.TestNoneAssets_NoneTest_txt.GetFileInfo();
		Assert.True(file.Exists);
	}

	[Fact]
	public void GetMatchesIsAccessible()
	{
		var allNones = Nones.GetMatches("**/*").ToArray();

		Assert.Equivalent(new[]
		{
			None.TestNoneAssets_NoneTest_txt
		}, allNones);
	}

	[Theory]
	[Nones.FromPattern("testnoneassets/**")]
	public void PrintNonePath(None file)
	{
		testOutputHelper.WriteLine(file.GetNoneFilePath());
	}
}