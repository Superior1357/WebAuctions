using WARSApplication.Data.DataModel.Business;
using WARSApplication.Repository;

namespace UnitTests;

public class MockRepository<T>(ICollection<T> repoItems) : IRepository<T>
{
    private ICollection<T> _repoContent = repoItems.ToList();
    public ICollection<T> CommitedRepoContent { get; private set; } = repoItems.ToList();
    public bool AllChangesCommited { get; private set; }
    
    public IAsyncEnumerable<T> WhereAsync(Func<T, bool> condition)
    {
        throw new NotImplementedException();
    }

    public Task<T> AddAsync(T item)
    {
        AllChangesCommited = false;
        _repoContent.Add(item);
        return Task.FromResult(item);
    }
    

    public Task UpdateAsync(T item)
    {
        AllChangesCommited = false;
        throw new NotImplementedException();
    }

    public Task CommitAsync()
    {
        AllChangesCommited = true;
        CommitedRepoContent = _repoContent.ToList();
        return Task.CompletedTask;
    }
}

public class MockAuctionRepository(ICollection<Auction> repoItems) : MockRepository<Auction>(repoItems);