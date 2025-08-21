using AsasKit.Application.Abstractions.Persistence;
using AsasKit.Infrastructure.Data;

namespace AsasKit.Infrastructure.Persistence;

public sealed class UnitOfWork(AppDbContext db) : IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}