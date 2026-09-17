using System.Diagnostics;
using Backend.Data.SharedDataProperties;

namespace Backend.Data.DataModel.Business;

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

/// <summary>
/// Represents an English auction.
/// An English auction is an auction where the highest bidder wins.
/// Every bid has to be higher than the previous bid; the starting price is the lowest bid.
/// The auction is open until the end time.
/// </summary>
internal sealed record EnglishAuction(DateTimeOffset StartTime, DateTimeOffset EndTime) : Auction(StartTime, EndTime)
{
    public IAddOnlyCollection<Bid> Bids { get; private init; } = new AddOnlyCollection<Bid>();
    public required Price StartingPrice { get; init; }
    
    /// <summary>
    /// Database restore function.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="description"></param>
    /// <param name="seller"></param>
    /// <param name="startTime"></param>
    /// <param name="endTime"></param>
    /// <param name="bids"></param>
    /// <param name="startingPrice"></param>
    /// <returns></returns>
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

/// <summary>
/// Represents a Dutch auction.
/// A Dutch auction is an auction where the (only) bidder wins.
/// The auction is open until the end time or until a bid is made.
/// The minimal bid price is decremented by a fixed rate every fixed time.
/// </summary>
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

    /// <summary>
    /// Database restore function.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="description"></param>
    /// <param name="seller"></param>
    /// <param name="startTime"></param>
    /// <param name="endTime"></param>
    /// <param name="bid"></param>
    /// <param name="startingPrice"></param>
    /// <param name="priceDecrementRate"></param>
    /// <param name="priceDecrementFrequency"></param>
    /// <returns></returns>
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

/// <summary>
/// Represents a First Price Sealed Bid auction.
/// A First Price Sealed Bid auction is an auction where the highest bidder wins.
/// Every bidder can bid only once.
/// The minimal bid is not revealed.
/// </summary>
/// <param name="StartTime"></param>
/// <param name="EndTime"></param>
internal sealed record FirstPriceSealedBidAuction(DateTimeOffset StartTime, DateTimeOffset EndTime) : Auction(StartTime, EndTime)
{
    /// <summary>
    /// Database restore function.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="description"></param>
    /// <param name="seller"></param>
    /// <param name="bids"></param>
    /// <param name="startTime"></param>
    /// <param name="endTime"></param>
    /// <returns></returns>
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