// AsasKit.Modules.Uow/Options/UowOptions.cs
using System.Data;

namespace AsasKit.UOW.Options;

public sealed class UowOptions
{
    public IsolationLevel Isolation { get; set; } = IsolationLevel.ReadCommitted;
    public bool UseExecutionStrategy { get; set; } = true;  // EF retries for transient faults
    public bool UseSavepointsForNested { get; set; } = true; // nested UoW -> savepoints
    public bool TreatRequestsEndingWithQueryAsReadOnly { get; set; } = true; // "Query" -> no TX
}
