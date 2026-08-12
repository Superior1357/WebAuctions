using WARSApplication.Data.DataModel.Business;

namespace WARSApplication.Repository;

public interface IRepository<T>
{
    public IAsyncEnumerable<T> WhereAsync(Func<T, bool> condition);
    public Task AddAsync(T entity);
    public Task UpdateAsync(T entity);
    public Task CommitAsync();
}

public interface IAuctionRepository : IRepository<Auction>;
public interface IBidRepository : IRepository<Bid>;

public class AuctionRepository(AuctionDbContext db) : IAuctionRepository
{
    public IAsyncEnumerable<Auction> WhereAsync(Func<Auction, bool> condition) =>
        db.Auctions.Where(condition).ToAsyncEnumerable();
    public async Task AddAsync(Auction auction) => await db.Auctions.AddAsync(auction);
    public Task UpdateAsync(Auction auction) => throw new NotImplementedException();
    public async Task CommitAsync() => await db.SaveChangesAsync();
}

public class BidRepository : IBidRepository
{
    public IAsyncEnumerable<Bid> WhereAsync(Func<Bid, bool> condition)
    {
        throw new NotImplementedException();
    }

    public Task AddAsync(Bid entity)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(Bid entity)
    {
        throw new NotImplementedException();
    }

    public Task CommitAsync()
    {
        throw new NotImplementedException();
    }
}

