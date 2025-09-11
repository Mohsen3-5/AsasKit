using Asas.Core.EF;

namespace Asas.Permission.Domain.Entity;
public class AsasPermission : AsasEntity<Guid>
{
    public string Name { get; set; } = default!;       
    public string? DisplayName { get; set; }         
    public string? Description { get; set; }
    public string Group { get; set; } = "General";
    public bool IsEnabled { get; set; } = true;
}