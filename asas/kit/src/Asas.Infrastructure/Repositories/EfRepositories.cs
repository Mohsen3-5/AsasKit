using Microsoft.EntityFrameworkCore;
using Asas.Core.Abstractions;

namespace Asas.Infrastructure.Repositories;

public class EfRepository<TEntity, TKey, TDbContext> : IRepository<TEntity, TKey>
    where TEntity : class
    where TDbContext : DbContext
{
    protected readonly TDbContext _db;

    public EfRepository(TDbContext db) => _db = db;

    // --- Get ---
    public TEntity? GetById(TKey id) => _db.Set<TEntity>().Find(id);

    public async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default)
        => await _db.Set<TEntity>().FindAsync(new object[] { id }, ct);

    public IReadOnlyList<TEntity> GetAll() => _db.Set<TEntity>().ToList();

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken ct = default)
        => await _db.Set<TEntity>().ToListAsync(ct);

    // --- Add ---
    public void Add(TEntity entity) => _db.Set<TEntity>().Add(entity);

    public async Task AddAsync(TEntity entity, CancellationToken ct = default)
        => await _db.Set<TEntity>().AddAsync(entity, ct);

    public void AddRange(IEnumerable<TEntity> entities) => _db.Set<TEntity>().AddRange(entities);

    public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default)
        => await _db.Set<TEntity>().AddRangeAsync(entities, ct);

    // --- Update ---
    public void Update(TEntity entity) => _db.Set<TEntity>().Update(entity);

    public Task UpdateAsync(TEntity entity, CancellationToken ct = default)
    {
        _db.Set<TEntity>().Update(entity);
        return Task.CompletedTask; // EF tracks update immediately
    }

    // --- Remove ---
    public void Remove(TEntity entity) => _db.Set<TEntity>().Remove(entity);

    public void RemoveRange(IEnumerable<TEntity> entities) => _db.Set<TEntity>().RemoveRange(entities);

    // --- Save Changes ---
    public int SaveChanges() => _db.SaveChanges();

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
