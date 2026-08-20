namespace WARSApplication.Repository;

public interface IRepository<T>
{
    public Task<IEnumerable<T>> ListAsync();
    /// <summary>
    /// Adds entity to the repository. Returns created entity with id parameterized.
    /// </summary>
    /// <param name="entity">Added entity</param>
    /// <returns></returns>
    public Task<T> AddAsync(T entity);
    public Task CommitAsync();
}
// TODO: maybe refactor (tests are too dependent on the data model) 