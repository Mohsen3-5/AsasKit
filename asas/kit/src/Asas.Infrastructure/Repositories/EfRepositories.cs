using Microsoft.EntityFrameworkCore;
using Asas.Core.EF;
using Asas.Tenancy.Contracts;
using Asas.Core.Exceptions;
using Asas.Core.Paging;

namespace Asas.Infrastructure.Repositories;

public class EfRepository<TEntity, TDbContext> : IRepository<TEntity>
    where TEntity : Entity
    where TDbContext : DbContext
{
    protected readonly TDbContext _db;
    protected readonly ICurrentTenant _currentTenant;

    public EfRepository(TDbContext db, ICurrentTenant currentTenant) => (_db, _currentTenant) = (db, currentTenant);

    public IQueryable<TEntity> Query() => _db.Set<TEntity>().AsQueryable();

    public TEntity? GetById(Guid id) => _db.Set<TEntity>().Find(id);
    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Set<TEntity>().FindAsync(new object?[] { id }, ct);
    public IReadOnlyList<TEntity> GetAll() => _db.Set<TEntity>().ToList();

    public async Task<PagedResponse<TEntity>> GetPagedAsync(
    int pageNumber,
    int pageSize,
    CancellationToken ct = default)
    {
        var query = _db.Set<TEntity>();
        return await query.ToPagedResponseAsync(pageNumber, pageSize, ct);
    }

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken ct = default)
        => await _db.Set<TEntity>().ToListAsync(ct);

    public void Add(TEntity entity)
    {
        _db.Set<TEntity>().Add(entity);
    }

    public Task AddAsync(TEntity entity, CancellationToken ct = default)
    {
        return _db.Set<TEntity>().AddAsync(entity, ct).AsTask();
    }
    public void AddRange(IEnumerable<TEntity> entities)
    {
        _db.Set<TEntity>().AddRange(entities);
    }

    public Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default)
    {
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

}
