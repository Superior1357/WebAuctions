using System.Collections;
using Microsoft.EntityFrameworkCore;
using WARSApplication.Data.DataModel;
using WARSApplication.Data.DataModel.Business;
using WARSApplication.Data.SharedDataProperties;
using WARSApplication.Exceptions;

namespace WARSApplication.Repository;

internal sealed class AppRepository(AuctionDbContext dbContext) : IRepository<Auction>
{
    private sealed class ChangesTrackingAddOnlyCollection<T>(IEnumerable<T> initialCollection) : IAddOnlyCollection<T>
    {
        private readonly List<T> _items = initialCollection.ToList();
        private readonly List<T> _addedItems = [];
        public IReadOnlyCollection<T> AddedItems => _addedItems;
        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            _addedItems.Add(item);
            _items.Add(item);
        }

        public int Count => _items.Count;
    }
    
    private static Bid DataToBusinessBid(DataModelBid dataBid)
    => Bid.Restore(dataBid.Id, User.Restore(dataBid.Bidder.Id), dataBid.Value, dataBid.Timestamp);

    private IEnumerable<DataModelBid> DataBidsOfAnAuction(DataModelAuction dataAuction) =>
        dbContext.Bids.Where(b => b.AuctionId == dataAuction.Id);
    
    private Auction DataToBusinessAuction(DataModelAuction dataAuction)
    {
        var bids = DataBidsOfAnAuction(dataAuction).Select(DataToBusinessBid);
        var bidsCollection = new ChangesTrackingAddOnlyCollection<Bid>(bids);

        return dataAuction switch
        {
            EnglishDataModelAuction englishDataAuction => EnglishAuction.Restore(dataAuction.Id,
                dataAuction.Description, User.Restore(dataAuction.Seller.Id), dataAuction.StartTime,
                dataAuction.EndTime, bidsCollection, englishDataAuction.StartingPrice),
            
            DutchDataModelAuction dutchDataAuction => DutchAuction.Restore(dataAuction.Id, dataAuction.Description,
                User.Restore(dataAuction.Seller.Id), dataAuction.StartTime, dataAuction.EndTime, bidsCollection.FirstOrDefault(),
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

    private async Task<DataModelBid> BusinessToDataModelBid(Bid bid)
    {
        return new DataModelBid
        {
            Bidder = await dbContext.Users.FirstAsync(u => u.Id == bid.Bidder.Id),
            Value = bid.Value,
            Timestamp = bid.Time
        };
    }
    
    public async Task UpdateAsync(Auction updatedAuction)
    {
        var dataAuction = await dbContext.Auctions.FirstAsync(a => a.Id == updatedAuction.Id);
        dataAuction.EndTime = updatedAuction.EndTime;
        
        switch (dataAuction)
        {
            case EnglishDataModelAuction englishDataModelAuction:
                var englishBusinessAuction = (EnglishAuction) updatedAuction;
                var updatedCollection = englishBusinessAuction.Bids as ChangesTrackingAddOnlyCollection<Bid>;
                var addedBids = updatedCollection.AddedItems;
                
                if (addedBids.Count != 0)
                {
                    englishDataModelAuction.IncrementVersion();

                    foreach (var addedBid in addedBids)
                    {
                        var dataModelBid = await BusinessToDataModelBid(addedBid);
                        englishDataModelAuction.Bids.Add(dataModelBid);
                    }
                }
                
                break;
            case DutchDataModelAuction dutchDataModelAuction:
                var dutchBusinessAuction = (DutchAuction)updatedAuction;
                if (dutchBusinessAuction.Bid is not null)
                {
                    dutchDataModelAuction.IncrementVersion();
                    dutchDataModelAuction.Bids.Add(await BusinessToDataModelBid(dutchBusinessAuction.Bid));
                }

                break;
            case FirstPriceSealedBidDataModelAuction firstPriceSealedBidDataModelAuction:
                var firstPriceSealedBidBusinessAuction = (FirstPriceSealedBidAuction) updatedAuction;
                updatedCollection = firstPriceSealedBidBusinessAuction.Bids as ChangesTrackingAddOnlyCollection<Bid>;
                addedBids = updatedCollection.AddedItems;
                
                if (addedBids.Count != 0)
                {
                    firstPriceSealedBidDataModelAuction.IncrementVersion();

                    foreach (var addedBid in addedBids)
                    {
                        var dataModelBid = await BusinessToDataModelBid(addedBid);
                        firstPriceSealedBidDataModelAuction.Bids.Add(dataModelBid);
                    }
                }
                
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public async Task AddAsync(Auction entity)
    {
        var dataAuction = NewBusinessToNewDataAuction(entity);
        await dbContext.Auctions.AddAsync(dataAuction);
    }

    public async Task CommitAsync()
    {
        try
        {
            await dbContext.SaveChangesAsync();
        }
        
        catch (DbUpdateConcurrencyException)
        {
            var completionSource = new TaskCompletionSource();
            completionSource.SetException(new ConcurrencyException());
        }
    }
}
/* TODO list: 
- improve sent error messages
- improve frontend (so that it looks better)
- registration twice handling
NOT:
- authentication tests
*/