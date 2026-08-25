using Microsoft.EntityFrameworkCore;
using WARSApplication.Data.DataModel;
using WARSApplication.Data.DataModel.Business;
using WARSApplication.Data.SharedDataProperties;

namespace WARSApplication.Repository;

public sealed class AppRepository(AuctionDbContext dbContext) : IRepository<Auction>
{
    private static Bid DataToBusinessBid(DataModelBid dataBid)
    => Bid.Restore(dataBid.Id, User.Restore(dataBid.Bidder.Id), dataBid.Value, dataBid.Timestamp);
    
    private Auction DataToBusinessAuction(DataModelAuction dataAuction)
    {
        var bids = dbContext.Bids.Where(b => b.AuctionId == dataAuction.Id).AsEnumerable().Select(DataToBusinessBid);
        var bidsCollection = new AddOnlyCollection<Bid>(bids);

        return dataAuction switch
        {
            EnglishDataModelAuction englishDataAuction => EnglishAuction.Restore(dataAuction.Id,
                dataAuction.Description, User.Restore(dataAuction.Seller.Id), dataAuction.StartTime,
                dataAuction.EndTime, bidsCollection, englishDataAuction.StartingPrice),
            
            DutchDataModelAuction dutchDataAuction => DutchAuction.Restore(dataAuction.Id, dataAuction.Description,
                User.Restore(dataAuction.Seller.Id), dataAuction.StartTime, dataAuction.EndTime, bidsCollection,
                dutchDataAuction.StartingPrice, dutchDataAuction.PriceDecrementRate,
                dutchDataAuction.PriceDecrementFrequency),
            
            FirstPriceSealedBidDataModelAuction => FirstPriceSealedBidAuction.Restore(dataAuction.Id,
                dataAuction.Description, User.Restore(dataAuction.Seller.Id), bidsCollection, dataAuction.StartTime,
                dataAuction.EndTime),
            
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private DataModelAuction NewBusinessToNewDataAuction(Auction businessAuction)
    {
        var dataModelUser = dbContext.Users.First(u => u.Id == businessAuction.Seller.Id);

        return businessAuction switch
        {
            EnglishAuction englishAuction => new EnglishDataModelAuction
            {
                Bids = [],
                Description = businessAuction.Description,
                EndTime = businessAuction.EndTime,
                Seller = dataModelUser,
                StartTime = businessAuction.StartTime,
                StartingPrice = englishAuction.StartingPrice
            },
            
            DutchAuction dutchAuction => new DutchDataModelAuction
            {
                Bids = [],
                Description = businessAuction.Description,
                EndTime = businessAuction.EndTime,
                Seller = dataModelUser,
                StartTime = businessAuction.StartTime,
                StartingPrice = dutchAuction.StartingPrice,
                PriceDecrementRate = dutchAuction.PriceDecrementRate,
                PriceDecrementFrequency = dutchAuction.PriceDecrementFrequency
            },
            
            FirstPriceSealedBidAuction => new FirstPriceSealedBidDataModelAuction
            {
                Bids = [],
                Description = businessAuction.Description,
                EndTime = businessAuction.EndTime,
                Seller = dataModelUser,
                StartTime = businessAuction.StartTime
            },
            
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    public async Task<IEnumerable<Auction>> ListAsync()
    {
        var dataAuctions = dbContext.Auctions.Include(a => a.Bids).ThenInclude(b => b.Bidder).Include(a => a.Seller).AsAsyncEnumerable();
        var businessAuctions = new List<Auction>();

        await foreach (var dataAuction in dataAuctions)
        {
            var businessAuction = DataToBusinessAuction(dataAuction);
            businessAuctions.Add(businessAuction);
        }

        return businessAuctions;
    }

    public async Task UpdateAsync(Auction entity)
    {
        var dataAuction = await dbContext.Auctions.FirstAsync(a => a.Id == entity.Id);
        dataAuction.EndTime = entity.EndTime;
    }

    public async Task AddAsync(Auction entity)
    {
        var dataAuction = NewBusinessToNewDataAuction(entity);
        await dbContext.Auctions.AddAsync(dataAuction);
    }

    public async Task CommitAsync() // TODO: handle when cannot commit in the internal database
    {
        await dbContext.SaveChangesAsync();
    }
}