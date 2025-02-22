[![Stand With Ukraine](https://raw.githubusercontent.com/vshymanskyy/StandWithUkraine/main/banner2-direct.svg)](https://stand-with-ukraine.pp.ua)

A refined version of the original [EmbeddedResourceAccessGenerator](https://github.com/ChristophHornung/EmbeddedResourceGenerator) by [ChristophHornung](https://github.com/ChristophHornung) adapted for None items.

*Important*: The original EmbeddedResourceAccessGenerator is incompatible with this generator and must be removed before adding this generator.

# NoneItemAccessGenerator
[![NuGet version (Raffinert.NoneItemAccessGenerator)](https://img.shields.io/nuget/v/Raffinert.NoneItemAccessGenerator.svg?style=flat-square)](https://www.nuget.org/packages/Raffinert.NoneItemAccessGenerator/)

The Raffinert.NoneItemAccessGenerator is a code generator to allow easy access to all
none item files with **CopyToOutputDirectory attribute specified as Always or PreserveNewest**.

## Usage
Get the nuget package [here](https://www.nuget.org/packages/Raffinert.NoneItemAccessGenerator).

After referencing the `Raffinert.NoneItemAccessGenerator` nuget the code generation will
automatically create a class `Nones` in the root namespace of the project.

Together with the generated `None` enumeration there are several options to access
none item files:

E.g. for a `Test.txt` none item in the `TestAsset` folder:

- Via enum access through the `None` enum:

```csharp
// Via the generated extension methods on the enum
using Stream s = None.TestAsset_Test_txt.GetStream();
using StreamReader sr = None.TestAsset_Test_txt.GetReader();
string text = None.TestAsset_Test_txt.ReadAllText();
string textAsync = await None.TestAsset_Test_txt.ReadAllTextAsync(CancellationToken.None);
byte[] bytes = None.TestAsset_Test_txt.ReadAllBytes();
byte[] bytesAsync = await None.TestAsset_Test_txt.ReadAllBytesAsync(CancellationToken.None);
```

- Via enum access through the `None[FolderName]` enum:

```csharp
// Via the generated extension methods on the enum
using Stream s = None_TestAsset.Test_txt.GetStream();
using StreamReader sr = None_TestAsset.Test_txt.GetReader();
string text = None_TestAsset.Test_txt.ReadAllText();
string textAsync = await None_TestAsset.Test_txt.ReadAllTextAsync(CancellationToken.None);
byte[] bytes = None_TestAsset.Test_txt.ReadAllBytes();
byte[] bytesAsync = await None_TestAsset.Test_txt.ReadAllBytesAsync(CancellationToken.None);
```

### Using `GetMatches` for Pattern Matching

```csharp
var matches = Nones.GetMatches("**/*");
foreach (var none in matches)
{
    Console.WriteLine(none);
}
```


### xUnit integration

```csharp
[Theory]
[Nones.FromPattern("**/*test.txt")]
public void PrintNonePath(None file)
{
    testOutputHelper.WriteLine(file.GetNoneFilePath());
}
```

## See also:

* [Raffinert.ContentItemAccessGenerator](https://www.nuget.org/packages/Raffinert.ContentItemAccessGenerator)
* [Raffinert.EmbeddedResourceAccessGenerator](https://www.nuget.org/packages/Raffinert.EmbeddedResourceAccessGenerator)
