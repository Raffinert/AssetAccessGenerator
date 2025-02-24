﻿namespace Raffinert.AssetAccessGenerator.Tests.Tests;

using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

/// <summary>
/// The main tests. This file is deliberately not in the root folder to test that the generator works no
/// files in the csproj folder.
/// </summary>
public class EmbeddedAssetAccessGeneratorTests(ITestOutputHelper testOutputHelper)
{
	[Fact]
	public void TestTxtIsAccessible()
	{
		using var reader = EmbeddedResource.TestAssets_Test_txt.GetReader();
		Assert.Equal("Success", reader.ReadToEnd());

		using var reader2 = EmbeddedResource_TestAssets.Test_txt.GetReader();
		Assert.Equal("Success", reader2.ReadToEnd());
	}

	[Fact]
	public void TestWithSpacesTxtIsAccessible()
	{
		using var reader = EmbeddedResource.TestAssets_Test_With_Spaces_txt.GetReader();
		Assert.Equal("Success", reader.ReadToEnd());

		using var reader2 = EmbeddedResource_TestAssets.Test_With_Spaces_txt.GetReader();
		Assert.Equal("Success", reader2.ReadToEnd());
	}

	[Fact]
	public void SubfolderTestWithSpacesTxtIsAccessible()
	{
		using var reader = EmbeddedResource.TestAssets_Subfolder_With_Spaces_Test_With_Spaces_txt.GetReader();
		Assert.Equal("Success", reader.ReadToEnd());

		using var reader2 = EmbeddedResource_TestAssets_Subfolder_With_Spaces.Test_With_Spaces_txt.GetReader();
		Assert.Equal("Success", reader2.ReadToEnd());
	}

	[Fact]
	public void RootTestTxtIsAccessible()
	{
		using var reader = EmbeddedResource.TestAssets_Test_txt.GetReader();
		Assert.Equal("Success", reader.ReadToEnd());
	}

	[Fact]
	public void SubfolderTestTxtIsAccessible()
	{
		using var reader = EmbeddedResource.TestAssets_Subfolder_Test_txt.GetReader();
		Assert.Equal("Success", reader.ReadToEnd());

		using var reader2 = EmbeddedResource_TestAssets_Subfolder.Test_txt.GetReader();
		Assert.Equal("Success", reader2.ReadToEnd());
	}

	[Fact]
	public void InvalidCharsTxtIsAccessible()
	{
		using var reader = EmbeddedResource.TestAssets_2InvalidChars___txt.GetReader();
		Assert.Equal("Success", reader.ReadToEnd());

		var text = EmbeddedResource.TestAssets_2InvalidChars___txt.ReadAllText();
		Assert.Equal("Success", text);

		using var reader2 = EmbeddedResource_TestAssets._InvalidChars___txt.GetReader();
		Assert.Equal("Success", reader2.ReadToEnd());
	}

	[Fact]
	public void InvalidCharsSubfolderTxtIsAccessible()
	{
		typeof(EmbeddedResource).Assembly.GetManifestResourceNames().ToList().ForEach(s => Debug.WriteLine(s));
		using var reader = EmbeddedResource.TestAssets___InvalidChars_Test_txt.GetReader();
		Assert.Equal("Success", reader.ReadToEnd());

		using var reader2 = EmbeddedResource_TestAssets___InvalidChars.Test_txt.GetReader();
		Assert.Equal("Success", reader2.ReadToEnd());
	}

	[Fact]
	public async Task InvalidCharsRootTxtIsAccessible()
	{
		using var reader = EmbeddedResource._InvalidChars___txt.GetReader();
		Assert.Equal("Success", reader.ReadToEnd());

		var bytes1 = await EmbeddedResource._InvalidChars___txt.ReadAllBytesAsync(CancellationToken.None);

		Assert.Equivalent(new byte[] { 239, 187, 191, 83, 117, 99, 99, 101, 115, 115 }, bytes1);

		var bytes2 = EmbeddedResource._InvalidChars___txt.ReadAllBytes();

		Assert.Equivalent(new byte[] { 239, 187, 191, 83, 117, 99, 99, 101, 115, 115 }, bytes2);
	}

	[Fact]
	public void GetMatchesIsAccessible()
	{
		var allEmbeddedResources = EmbeddedResources.GetMatches("**/*").ToArray();

		Assert.Equivalent(new[]
		{
			EmbeddedResource.TestAssets___InvalidChars_Test_txt,
			EmbeddedResource.TestAssets_2InvalidChars___txt,
			EmbeddedResource.TestAssets_Subfolder_With_Spaces_Test_With_Spaces_txt,
			EmbeddedResource.TestAssets_Subfolder_Test_txt,
			EmbeddedResource.TestAssets_Test_With_Spaces_txt,
			EmbeddedResource.TestAssets_Test_txt,
			EmbeddedResource._InvalidChars___txt,
			EmbeddedResource.Test_txt
		}, allEmbeddedResources);
	}

	[Theory]
	[EmbeddedResources.FromPattern("**/2*")]
	public void PrintEmbeddedResource(EmbeddedResource file)
	{
		testOutputHelper.WriteLine(file.ToString());
	}
}