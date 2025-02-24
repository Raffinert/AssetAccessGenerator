﻿namespace Raffinert.AccessGenerator.Core;

using System.Collections;
using System.Collections.Immutable;

/// <summary>
/// Represents a resource item.
/// </summary>
internal record ResourceItem(
	string RelativePath,
	string IdentifierName,
	string ResourceName,
	ResourceKind ResourceKind)
{
	/// <summary>
	/// Gets the relative path of the resource item.
	/// </summary>
	public string RelativePath { get; } = RelativePath;

	/// <summary>
	/// Gets the name of the resource.
	/// </summary>
	public string ResourceName { get; } = ResourceName;

	/// <summary>
	/// Gets the identifier name of the resource item.
	/// </summary>
	public string IdentifierName { get; } = IdentifierName;

	/// <summary>
	/// Gets the kind of the resource.
	/// </summary>
	public ResourceKind ResourceKind { get; } = ResourceKind;
}

/// <summary>
/// Represents the kind of resource.
/// </summary>
internal enum ResourceKind
{
	Unspecified,
	EmbeddedResource,
	Content,
	None
}

/// <summary>
/// Represents the context for resource generation.
/// </summary>
internal record GenerationContext(
	ImmutableArray<ResourceItem> Resources,
	string RootNamespace,
	ImmutableArray<string> MatchesLiterals,
	bool IsXunitDataAttributeAvailable) : IEnumerable<ResourceItem>
{
	private ImmutableArray<ResourceItem> Resources = Resources;

	public ImmutableArray<string> MatchesLiterals { get; } = MatchesLiterals;

	/// <summary>
	/// Gets the root namespace.
	/// </summary>
	public string RootNamespace { get; } = RootNamespace;

	public bool IsXunitDataAttributeAvailable { get; } = IsXunitDataAttributeAvailable;

	/// <summary>
	/// Creates a new resource generation context with the specified resource kind.
	/// </summary>
	/// <param name="kind">The resource kind.</param>
	/// <returns>A new resource generation context.</returns>
	public GenerationContext With(ResourceKind kind)
	{
		return this with { Resources = this.Resources.Where(x => x.ResourceKind == kind).ToImmutableArray() };
	}

	/// <inheritdoc />
	public IEnumerator<ResourceItem> GetEnumerator()
	{
		return this.Resources.AsEnumerable().GetEnumerator();
	}

	/// <inheritdoc />
	IEnumerator IEnumerable.GetEnumerator()
	{
		return this.GetEnumerator();
	}

	/// <summary>
	/// Gets a value indicating whether the resource generation context is empty.
	/// </summary>
	public bool IsEmpty => this.Resources.IsEmpty;
}
