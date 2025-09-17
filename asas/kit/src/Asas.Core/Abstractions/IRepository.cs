using Asas.Core.EF;
using Asas.Core.Paging;

public interface IRepository<TEntity> where TEntity : Entity
{
    IQueryable<TEntity> Query();
    TEntity? GetById(Guid id);
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
    IReadOnlyList<TEntity> GetAll();
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken ct = default);
    Task<PagedResponse<TEntity>> GetPagedAsync(
    int pageNumber,
    int pageSize,
    CancellationToken ct = default);
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
