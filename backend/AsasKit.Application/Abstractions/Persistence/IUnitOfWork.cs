namespace AsasKit.Application.Abstractions.Persistence;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct);
}