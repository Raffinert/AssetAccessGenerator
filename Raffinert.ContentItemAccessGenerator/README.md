[![StandWithUkraine](https://raw.githubusercontent.com/vshymanskyy/StandWithUkraine/main/badges/StandWithUkraine.svg)](https://github.com/vshymanskyy/StandWithUkraine/blob/main/docs/README.md)

A refined version of the original [EmbeddedResourceAccessGenerator](https://github.com/ChristophHornung/EmbeddedResourceGenerator) by [ChristophHornung](https://github.com/ChristophHornung) adapted for Content items.

*Important*: The original EmbeddedResourceAccessGenerator is incompatible with this generator and must be removed before adding this generator.

# ContentItemAccessGenerator
[![NuGet version (Raffinert.ContentItemAccessGenerator)](https://img.shields.io/nuget/v/Raffinert.ContentItemAccessGenerator.svg?style=flat-square)](https://www.nuget.org/packages/Raffinert.ContentItemAccessGenerator/)

The Raffinert.ContentItemAccessGenerator is a code generator to allow easy access to all
content files with **CopyToOutputDirectory attribute specified as Always or PreserveNewest**.

## Usage
Get the nuget package [here](https://www.nuget.org/packages/Raffinert.ContentItemAccessGenerator).

After referencing the `Raffinert.ContentItemAccessGenerator` nuget the code generation will
automatically create a class `Contents` in the root namespace of the project.

Together with the generated `Content` enumeration there are several options to access
content files:

E.g. for a `Test.txt` content item in the `TestAsset` folder:

- Via enum access through the `Content` enum:

```csharp
    // Via the generated extension methods on the enum
    using Stream s = Content.TestAsset_Test_txt.GetStream();
    using StreamReader sr = Content.TestAsset_Test_txt.GetReader();
    string text = Content.TestAsset_Test_txt.ReadAllText();
    string textAsync = await Content.TestAsset_Test_txt.ReadAllTextAsync(CancellationToken.None);
    byte[] bytes = Content.TestAsset_Test_txt.ReadAllBytes();
    byte[] bytesAsync = await Content.TestAsset_Test_txt.ReadAllBytesAsync(CancellationToken.None);
```

- Via enum access through the `Content[FolderName]` enum:

```csharp
    // Via the generated extension methods on the enum
    using Stream s = Content_TestAsset.Test_txt.GetStream();
    using StreamReader sr = Content_TestAsset.Test_txt.GetReader();
    string text = Content_TestAsset.Test_txt.ReadAllText();
    string textAsync = await Content_TestAsset.Test_txt.ReadAllTextAsync(CancellationToken.None);
    byte[] bytes = Content_TestAsset.Test_txt.ReadAllBytes();
    byte[] bytesAsync = await Content_TestAsset.Test_txt.ReadAllBytesAsync(CancellationToken.None);
```

### Using `GetMatches` for Pattern Matching

```csharp
    var matches = Contents.GetMatches("**/*?t/*");
    foreach (var content in matches)
    {
        Console.WriteLine(content);
    }
```


### xUnit integration

```csharp
    [Theory]
    [Contents.FromPattern("**/**/cont*")]
    public void PrintContentPath(Content file)
    {
        testOutputHelper.WriteLine(file.GetContentFilePath());
    }
```

## See also:

* [Raffinert.NoneItemAccessGenerator](https://www.nuget.org/packages/Raffinert.NoneItemAccessGenerator)
* [Raffinert.EmbeddedResourceAccessGenerator](https://www.nuget.org/packages/Raffinert.EmbeddedResourceAccessGenerator)