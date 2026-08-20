using WARSApplication.Data.DataModel.Business;
namespace WARSApplication.Repository;

public sealed class AppRepository(AuctionDbContext dbContext, TimeProvider timeProvider) : IRepository<Auction>
{
    public Task<IEnumerable<Auction>> ListAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Auction> AddAsync(Auction entity)
    {
        throw new NotImplementedException();
    }

    Task IRepository<Auction>.CommitAsync() // TODO: handle when cannot commit in the internal database
    {
        throw new NotImplementedException();
    }

    private record RepoAuction(AppRepository CorrespondingRepository) : Auction
    {
        public override required DateTimeOffset EndTime { get; set; }
        public override IEnumerable<Bid> GetBids()
        {
            throw new NotImplementedException();
        }
    }
}