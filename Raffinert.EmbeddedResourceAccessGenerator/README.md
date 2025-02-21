[![StandWithUkraine](https://raw.githubusercontent.com/vshymanskyy/StandWithUkraine/main/badges/StandWithUkraine.svg)](https://github.com/vshymanskyy/StandWithUkraine/blob/main/docs/README.md)

A refined version of the original [EmbeddedResourceAccessGenerator](https://github.com/ChristophHornung/EmbeddedResourceGenerator) by [ChristophHornung](https://github.com/ChristophHornung)

*Important*: The original EmbeddedResourceAccessGenerator is incompatible with this generator and must be removed before adding this generator.

Improvements in the EmbeddedResourceAccessGenerator:

* Fixed a bug when the generator generates code for all AdditionalFiles, not EmbeddedResources only. This means that the original EmbeddedResourceAccessGenerator is incompatible with all three generators and must be removed before adding those generators.
* Removed properties from the EmbeddedResources class, because it's an anti-pattern to create disposable resources on every get property access.
* The GetReader method now creates the StreamReader with the option leaveOpen: false. It disposes the stream together with the StreamReader.
* Added synchronous and asynchronous extension methods ReadAllText, ReadAllTextAsync, ReadAllBytes, ReadAllBytesAsync.

# EmbeddedResourceAccessGenerator
[![NuGet version (Raffinert.EmbeddedResourceAccessGenerator)](https://img.shields.io/nuget/v/Raffinert.EmbeddedResourceAccessGenerator.svg?style=flat-square)](https://www.nuget.org/packages/Raffinert.EmbeddedResourceAccessGenerator/)

The Raffinert.EmbeddedResourceAccessGenerator is a code generator to allow easy access to all
embedded resources.

## Usage
Get the nuget package [here](https://www.nuget.org/packages/Raffinert.EmbeddedResourceAccessGenerator).

After referencing the `Raffinert.EmbeddedResourceAccessGenerator` nuget the code generation will
automatically create a class `EmbeddedResources` in the root namespace of the project.

Together with the generated `EmbeddedResource` enumeration there are several options to access
embedded resources:

E.g. for a `Test.txt` embedded resource in the `TestAsset` folder:

- Via enum access through the `EmbeddedResource` enum:

```csharp
    // Via the generated extension methods on the enum
    using Stream s = EmbeddedResource.TestAsset_Test_txt.GetStream();
    using StreamReader sr = EmbeddedResource.TestAsset_Test_txt.GetReader();
    string text = EmbeddedResource.TestAsset_Test_txt.ReadAllText();
    string textAsync = await EmbeddedResource.TestAsset_Test_txt.ReadAllTextAsync(CancellationToken.None);
    byte[] bytes = EmbeddedResource.TestAsset_Test_txt.ReadAllBytes();
    byte[] bytesAsync = await EmbeddedResource.TestAsset_Test_txt.ReadAllBytesAsync(CancellationToken.None);

```

- Via enum access through the `EmbeddedResource[FolderName]` enum:

```csharp
    // Via the generated extension methods on the enum
    using Stream s = EmbeddedResource_TestAsset.Test_txt.GetStream();
    using StreamReader sr = EmbeddedResource_TestAsset.Test_txt.GetReader();
    string text = EmbeddedResource_TestAsset.Test_txt.ReadAllText();
    string textAsync = await EmbeddedResource_TestAsset.Test_txt.ReadAllTextAsync(CancellationToken.None);
    byte[] bytes = EmbeddedResource_TestAsset.Test_txt.ReadAllBytes();
    byte[] bytesAsync = await EmbeddedResource_TestAsset.Test_txt.ReadAllBytesAsync(CancellationToken.None);
```

### Using `GetMatches` for Pattern Matching

```csharp
    var matches = EmbeddedResources.GetMatches("**/*e?/*");
    foreach (var resource in matches)
    {
        Console.WriteLine(resource);
    }
```


### xUnit integration

```csharp
    [Theory]
    [EmbeddedResources.FromPattern("**/**/*")]
    public void PrintEmbeddedResource(EmbeddedResource file)
    {
        testOutputHelper.WriteLine(file.ToString());
    }
```

## Motivation
Instead of using magic strings in the resource access code that may point to non-existant
resources this generator guarantees resources to exist and code to not compile when they are
removed.

Grouping the resources via their path adds path specific enums, e.g. to easily write tests
for all embedded resource in a subfolder.

Also it saves quite a bit of typing effort.

## See also:

* [Raffinert.ContentItemAccessGenerator](https://www.nuget.org/packages/Raffinert.ContentItemAccessGenerator)
* [Raffinert.NoneItemAccessGenerator](https://www.nuget.org/packages/Raffinert.NoneItemAccessGenerator)