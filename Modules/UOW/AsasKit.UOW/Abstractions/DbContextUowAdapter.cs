using System.Data;
using AsasKit.Shared.Messaging.Abstractions;   // IEventPublisher
using AsasKit.Shared.Messaging.Domain;        // IAggregateRoot, IDomainEvent, IIntegrationEvent
using AsasKit.UOW.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;

namespace AsasKit.UOW.Abstractions;

public sealed class DbContextUowAdapter<TDbContext> : IUnitOfWork
    where TDbContext : DbContext
{
    private readonly TDbContext _db;
    private readonly IEventPublisher _events;
    private readonly UowOptions _opt;

    public DbContextUowAdapter(
        TDbContext db,
        IEventPublisher events,
        IOptions<UowOptions> opt)
    {
        _db = db;
        _events = events;
        _opt = opt.Value;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        ApplyAuditing();

        var domainEvents = Dequeue<IDomainEvent>(a => a.DequeueDomainEvents());
        var integrationEvents = Dequeue<IIntegrationEvent>(a => a.DequeueIntegrationEvents()); // TODO: enqueue to outbox if you have one

        var rows = await _db.SaveChangesAsync(ct);

        foreach (var e in domainEvents)
            await _events.PublishDomainAsync(e, ct);

        if (_db.ChangeTracker.HasChanges())
            rows += await _db.SaveChangesAsync(ct);

        return rows;
    }

    public Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> work,
        IsolationLevel isolation = IsolationLevel.ReadCommitted,
        CancellationToken ct = default)
        => ExecuteInTransactionAsync<object?>(async _ => { await work(ct); return null; }, isolation, ct);

    public async Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> work,
        IsolationLevel isolation = IsolationLevel.ReadCommitted,
        CancellationToken ct = default)
    {
        var strategy = _opt.UseExecutionStrategy ? _db.Database.CreateExecutionStrategy() : null;

        async Task<T> Body()
        {
            await using var tx = await _db.Database.BeginTransactionAsync(isolation, ct);
            try
            {
                var result = await work(ct);
                await SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                return result;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        return strategy is null ? await Body() : await strategy.ExecuteAsync(Body);
    }

    private IReadOnlyCollection<T> Dequeue<T>(Func<IAggregateRoot, IReadOnlyCollection<T>> sel) =>
        _db.ChangeTracker.Entries<IAggregateRoot>().SelectMany(e => sel(e.Entity)).ToArray();

    private void ApplyAuditing()
    {
        // set CreatedAt/UpdatedAt/TenantId/etc. here if you want
    }
}
