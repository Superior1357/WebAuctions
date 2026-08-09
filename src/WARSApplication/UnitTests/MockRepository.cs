using WARSApplication.Data.DataModel.Business;
using WARSApplication.Repository;

namespace UnitTests;

public class MockAuctionRepository(ICollection<Auction> repoItems) : IAuctionRepository
{
    private ICollection<Auction> _repoContent = repoItems.ToList();
    public ICollection<Auction> CommitedRepoContent { get; private set; } = repoItems.ToList();
    public bool AllChangesCommited { get; private set; }
    
    public IAsyncEnumerable<Auction> WhereAsync(Func<Auction, bool> condition)
    {
        throw new NotImplementedException();
    }

    public Task AddAsync(Auction auction)
    {
        AllChangesCommited = false;
        _repoContent.Add(auction);
        return Task.CompletedTask;
    }
    

    public Task UpdateAsync(Auction auction)
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