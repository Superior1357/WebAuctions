using WARSApplication.Data.DataModel.Business;
namespace WARSApplication.Repository;

public class AppRepository(AuctionDbContext dbContext) : IRepository<Auction>, IRepository<User>
{
    public IAsyncEnumerable<Auction> WhereAsync(Func<Auction, bool> condition)
    {
        throw new NotImplementedException();
    }

    public Task<Auction> AddAsync(Auction entity)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<User> WhereAsync(Func<User, bool> condition)
    {
        throw new NotImplementedException();
    }

    public Task<User> AddAsync(User entity)
    {
        throw new NotImplementedException();
    }

    Task IRepository<User>.CommitAsync()
    {
        throw new NotImplementedException();
    }

    Task IRepository<Auction>.CommitAsync()
    {
        throw new NotImplementedException();
    }
}

