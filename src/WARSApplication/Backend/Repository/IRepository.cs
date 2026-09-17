namespace Backend.Repository;

/// <summary>
/// Repository pattern interface
/// </summary>
/// <typeparam name="T"></typeparam>
internal interface IRepository<T>
{
    /// <summary>
    /// Asynchronously returns all entities of type T stored in the repository.
    /// </summary>
    /// <returns>all entities of type T stored in the repository</returns>
    public Task<IEnumerable<T>> ListAsync();
    
    /// <summary>
    /// Asynchronously updates entity in the repository.
    /// </summary>
    /// <param name="updatedEntity">the entity whose parameters have been updated</param>
    public Task UpdateAsync(T updatedEntity);
    
    /// <summary>
    /// Asynchronously adds a new entity to the repository.
    /// </summary>
    /// <param name="entity">Added entity</param>
    public Task AddAsync(T entity);
    
    /// <summary>
    /// Commit all changes to the repository.
    /// </summary>
    /// <returns></returns>
    public Task CommitAsync();
}