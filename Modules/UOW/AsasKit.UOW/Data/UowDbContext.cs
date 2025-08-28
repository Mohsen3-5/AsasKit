// AsasKit.UOW/Data/UowDbContext.cs
using System.Data;
using AsasKit.Shared.Messaging.Abstractions;   // IEventPublisher
using AsasKit.Shared.Messaging.Domain;
using AsasKit.UOW.Abstractions;
using AsasKit.UOW.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AsasKit.UOW.Data;

public abstract class UowDbContext : DbContext, IUnitOfWork
{
    private readonly IEventPublisher _events;
    private readonly UowOptions _opt;

    protected UowDbContext(
        DbContextOptions options,
        IEventPublisher events,
        IOptions<UowOptions> opt) : base(options)
    {
        _events = events;
        _opt = opt.Value;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        ApplyAuditing();

        var domainEvents = Dequeue<IDomainEvent>(a => a.DequeueDomainEvents());
        var integrationEvents = Dequeue<IIntegrationEvent>(a => a.DequeueIntegrationEvents()); // ready for outbox

        var rows = await base.SaveChangesAsync(ct);

        foreach (var e in domainEvents)
            await _events.PublishDomainAsync(e, ct);

        if (ChangeTracker.HasChanges())
            rows += await base.SaveChangesAsync(ct);

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
        var strategy = _opt.UseExecutionStrategy ? Database.CreateExecutionStrategy() : null;

        async Task<T> Body()
        {
            if (Database.CurrentTransaction is not null && _opt.UseSavepointsForNested)
                return await WithSavepointAsync(work, ct);

            await using var tx = await Database.BeginTransactionAsync(isolation, ct);
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

    private async Task<T> WithSavepointAsync<T>(Func<CancellationToken, Task<T>> work, CancellationToken ct)
    {
        var tx = Database.CurrentTransaction!;
        var sp = $"sp_{Guid.NewGuid():N}";
        await tx.CreateSavepointAsync(sp, ct);
        try
        {
            var result = await work(ct);
            await SaveChangesAsync(ct);
            await tx.ReleaseSavepointAsync(sp, ct);
            return result;
        }
        catch
        {
            await tx.RollbackToSavepointAsync(sp, ct);
            throw;
        }
    }

    private IReadOnlyCollection<T> Dequeue<T>(Func<IAggregateRoot, IReadOnlyCollection<T>> sel) =>
        ChangeTracker.Entries<IAggregateRoot>().SelectMany(e => sel(e.Entity)).ToArray();

    protected virtual void ApplyAuditing() { /* set Created/Updated/Tenant etc. if you want */ }
}
