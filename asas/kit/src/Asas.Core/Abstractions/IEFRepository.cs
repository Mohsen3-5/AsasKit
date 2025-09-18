using Asas.Core.EF;
using Asas.Core.Paging;

public interface IEFRepository<TEntity> where TEntity : Entity
{
    IQueryable<TEntity> Query();
    TEntity? GetById(Guid id);
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
    IReadOnlyList<TEntity> GetAll(Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null);
    Task<IReadOnlyList<TEntity>> GetAllAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null, CancellationToken ct = default);
    Task<PagedResponse<TEntity>> GetPagedAsync(PagedRequest request, Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null, CancellationToken ct = default);
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
