using Microsoft.EntityFrameworkCore;
using Asas.Core.EF;
using Asas.Tenancy.Contracts;

namespace Asas.Infrastructure.Repositories;

public class EfRepository<TEntity, TDbContext> : IRepository<TEntity>
    where TEntity : Entity
    where TDbContext : DbContext
{
    protected readonly TDbContext _db;
    protected readonly ICurrentTenant _currentTenant;

    public EfRepository(TDbContext db, ICurrentTenant currentTenant) => (_db, _currentTenant) = (db, currentTenant);

    public IReadOnlyList<TEntity> GetAll() => _db.Set<TEntity>().ToList();

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken ct = default)
        => await _db.Set<TEntity>().ToListAsync(ct);

    public void Add(TEntity entity)
    {
        ApplyTenant(entity);
        _db.Set<TEntity>().Add(entity);
    }

    public Task AddAsync(TEntity entity, CancellationToken ct = default)
    {
        ApplyTenant(entity);
        return _db.Set<TEntity>().AddAsync(entity, ct).AsTask();
    }
    public void AddRange(IEnumerable<TEntity> entities)
    {
        foreach (var e in entities) ApplyTenant(e!);
        _db.Set<TEntity>().AddRange(entities);
    }

    public Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default)
    {
        foreach (var e in entities) ApplyTenant(e!);
        return _db.Set<TEntity>().AddRangeAsync(entities, ct);
    }

    public void Update(TEntity entity) => _db.Set<TEntity>().Update(entity);

    public Task UpdateAsync(TEntity entity, CancellationToken ct = default)
    {
        _db.Set<TEntity>().Update(entity);
        return Task.CompletedTask;
    }

    public void Remove(TEntity entity) => _db.Set<TEntity>().Remove(entity);

    public void RemoveRange(IEnumerable<TEntity> entities) => _db.Set<TEntity>().RemoveRange(entities);

    public int SaveChanges() => _db.SaveChanges();

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);


    private void ApplyTenant(object entity)
    {
        if (!_currentTenant.IsSet) return;

        if (entity is Entity e && e.TenantId is null)
            e.TenantId = _currentTenant.Id;
    }

}
