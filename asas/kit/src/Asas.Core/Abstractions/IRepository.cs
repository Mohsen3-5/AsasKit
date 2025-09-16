using Asas.Core.EF;

public interface IRepository<TEntity> where TEntity : Entity
{
    IReadOnlyList<TEntity> GetAll();
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken ct = default);

    void Add(TEntity entity);
    Task AddAsync(TEntity entity, CancellationToken ct = default);

    void AddRange(IEnumerable<TEntity> entities);
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default);

    void Update(TEntity entity);
    Task UpdateAsync(TEntity entity, CancellationToken ct = default);

    void Remove(TEntity entity);
    void RemoveRange(IEnumerable<TEntity> entities);

    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
