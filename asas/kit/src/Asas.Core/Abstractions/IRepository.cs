namespace Asas.Core.Abstractions;

public interface IRepository<TEntity, TKey> where TEntity : class
{
    // --- Get ---
    TEntity? GetById(TKey id);
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default);

    IReadOnlyList<TEntity> GetAll();
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken ct = default);

    // --- Add ---
    void Add(TEntity entity);
    Task AddAsync(TEntity entity, CancellationToken ct = default);

    void AddRange(IEnumerable<TEntity> entities);
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default);

    // --- Update ---
    void Update(TEntity entity);
    Task UpdateAsync(TEntity entity, CancellationToken ct = default);

    // --- Remove ---
    void Remove(TEntity entity);
    void RemoveRange(IEnumerable<TEntity> entities);

    // --- Save Changes ---
    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
