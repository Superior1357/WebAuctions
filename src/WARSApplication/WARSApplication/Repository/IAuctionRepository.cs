using WARSApplication.Data.DataModel.Business;

namespace WARSApplication.Repository;

public interface IAuctionRepository
{
    public IAsyncEnumerable<Auction> AuctionsWhereAsync(Func<Auction, bool> condition);
    public Task AddAsync(Auction auction);
    public Task CommitAsync();
}

public class AuctionRepository(AuctionDbContext db) : IAuctionRepository
{
    public IAsyncEnumerable<Auction> AuctionsWhereAsync(Func<Auction, bool> condition) =>
        db.Auctions.Where(condition).ToAsyncEnumerable();
    
    public async Task AddAsync(Auction auction) => await db.Auctions.AddAsync(auction);
    public async Task CommitAsync() => await db.SaveChangesAsync();
}

