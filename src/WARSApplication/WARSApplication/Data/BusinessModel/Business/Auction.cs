using System.Diagnostics;
using WARSApplication.Data.SharedDataProperties;

namespace WARSApplication.Data.DataModel.Business;

/// <summary>
/// Base class for generic business auction representation. When creating a new auction with a not yet-assigned id,
/// the object should be created with the default constructor.
/// Restore function should be used to restore an object that has been persisted, for instance, to the database.
/// </summary>
internal abstract record Auction
{
    protected Auction(DateTimeOffset StartTime, DateTimeOffset EndTime)
    {
        Debug.Assert(StartTime < EndTime);
        
        this.StartTime = StartTime;
        this.EndTime = EndTime;
    }
    public AuctionId Id { get; protected init; }
    public AuctionDescription Description { get; init; } = string.Empty;
    public required User Seller { get; init; }
    public DateTimeOffset StartTime { get; }
    public DateTimeOffset EndTime { get; set; }
}


internal sealed record EnglishAuction(DateTimeOffset StartTime, DateTimeOffset EndTime) : Auction(StartTime, EndTime)
{
    public IAddOnlyCollection<Bid> Bids { get; private init; } = new AddOnlyCollection<Bid>();
    public required Price StartingPrice { get; init; }
    
    public static EnglishAuction Restore(
            AuctionId id,
            AuctionDescription description,
            User seller,
            DateTimeOffset startTime,
            DateTimeOffset endTime,
            IAddOnlyCollection<Bid> bids,
            Price startingPrice
        ) => new (startTime, endTime)
        {
            Id = id,
            Description = description,
            Seller = seller,
            Bids = bids,
            StartingPrice = startingPrice
        };
}

internal sealed record DutchAuction(DateTimeOffset StartTime, DateTimeOffset EndTime) : Auction(StartTime, EndTime)
{
    public required Price StartingPrice { get; init; }
    public required Price PriceDecrementRate { get; init; }
    public required TimeSpan PriceDecrementFrequency { get; init; }
    public Bid? Bid
    {
        get;
        set
        {
            if (Bid != null)
            {
                throw new InvalidOperationException("Bid already set");
            }
            
            field = value;
        }
    }

    public static DutchAuction Restore(
        AuctionId id,
        AuctionDescription description,
        User seller,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        Bid? bid,
        Price startingPrice,
        Price priceDecrementRate,
        TimeSpan priceDecrementFrequency
    ) => new (startTime, endTime)
    {
        Id = id,
        Description = description,
        Seller = seller,
        Bid = bid,
        StartingPrice = startingPrice,
        PriceDecrementRate = priceDecrementRate, 
        PriceDecrementFrequency = priceDecrementFrequency
    };
}

internal sealed record FirstPriceSealedBidAuction(DateTimeOffset StartTime, DateTimeOffset EndTime) : Auction(StartTime, EndTime)
{
    public static FirstPriceSealedBidAuction Restore(
        AuctionId id, 
        AuctionDescription description,
        User seller, 
        IAddOnlyCollection<Bid> bids,
        DateTimeOffset startTime,
        DateTimeOffset endTime) =>
        new (startTime, endTime)
        {
            Id = id,
            Description = description,
            Seller = seller,
            Bids = bids
        };
    
    public IAddOnlyCollection<Bid> Bids { get; private init; } = new AddOnlyCollection<Bid>();
}