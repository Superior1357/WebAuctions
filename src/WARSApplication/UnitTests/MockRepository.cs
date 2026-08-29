using WARSApplication.Data.DataModel.Business;
using WARSApplication.Repository;

namespace UnitTests;

public class MockRepository<T>(ICollection<T> repoItems) : IRepository<T>
{
    private readonly ICollection<T> _repoContent = repoItems.ToList();
    public ICollection<T> CommitedRepoContent { get; private set; } = repoItems.ToList();
    public bool AllChangesCommited { get; private set; }
    public ICollection<T> UpdatesCalled { get; } = new List<T>();
    
    public Task<IEnumerable<T>> ListAsync() => Task.FromResult(CommitedRepoContent.AsEnumerable());

    public Task AddAsync(T item)
    {
        AllChangesCommited = false;
        _repoContent.Add(item);
        return Task.FromResult(item);
    }

    public Task UpdateAsync(T item)
    {
        AllChangesCommited = false;
        UpdatesCalled.Add(item);
        return Task.CompletedTask;
    }

    public Task CommitAsync()
    {
        AllChangesCommited = true;
        CommitedRepoContent = _repoContent.ToList();
        return Task.CompletedTask;
    }
}

public class MockAuctionRepository(ICollection<Auction> repoItems) : MockRepository<Auction>(repoItems);