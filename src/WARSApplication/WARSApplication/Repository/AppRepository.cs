using WARSApplication.Data.DataModel.Business;
namespace WARSApplication.Repository;

public sealed class AppRepository(AuctionDbContext dbContext) : IRepository<Auction>
{
    public Task<IEnumerable<Auction>> ListAsync()
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(Auction entity)
    {
        throw new NotImplementedException();
    }

    public Task AddAsync(Auction entity)
    {
        throw new NotImplementedException();
    }

    public Task CommitAsync() // TODO: handle when cannot commit in the internal database
    {
        throw new NotImplementedException();
    }
}