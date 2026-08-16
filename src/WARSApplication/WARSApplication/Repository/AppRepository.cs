using WARSApplication.Data.DataModel.Business;
namespace WARSApplication.Repository;

public class AppRepository(AuctionDbContext dbContext) : IRepository<Auction>
{
    public IAsyncEnumerable<Auction> WhereAsync(Func<Auction, bool> condition)
    {
        throw new NotImplementedException();
    }

    public Task<Auction> AddAsync(Auction entity)
    {
        throw new NotImplementedException();
    }

    Task IRepository<Auction>.CommitAsync()
    {
        throw new NotImplementedException();
    }
}

