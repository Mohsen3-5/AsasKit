// Domain/SortDescriptor.cs
namespace AsasKit.Core.Domain;

/// <summary>Describes a sort by a field name with optional descending order.</summary>
public sealed record SortDescriptor(string Field, bool Desc = false);
