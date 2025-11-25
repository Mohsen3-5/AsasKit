namespace Asas.Identity.Domain.Entities;
public class EmailConfirmationCode
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public AsasUser User { get; set; } = default!;

    public string Code { get; set; } = null!; // store as string, e.g. "482913"

    public DateTime CreatedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }

    public int Attempts { get; set; }
    public bool Used { get; set; }
    public string Purpose { get; set; } = "Email";
}
