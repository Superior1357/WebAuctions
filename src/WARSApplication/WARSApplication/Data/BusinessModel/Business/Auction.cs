using WARSApplication.Data.SharedDataProperties;

namespace WARSApplication.Data.DataModel.Business;

public abstract record Auction(DateTimeOffset StartTime, DateTimeOffset EndTime)
{
    public AuctionId Id { get; protected init; }
    public AuctionDescription Description { get; init; } = string.Empty;
    public required User Seller { get; init; }
    public DateTimeOffset StartTime { get; } = StartTime;
    public DateTimeOffset EndTime { get; set; } = EndTime;
    public IAddOnlyCollection<Bid> Bids { get; protected init; } = new AddOnlyCollection<Bid>();
}

public sealed record EnglishAuction(DateTimeOffset StartTime, DateTimeOffset EndTime) : Auction(StartTime, EndTime)
{
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

public sealed record DutchAuction(DateTimeOffset StartTime, DateTimeOffset EndTime) : Auction(StartTime, EndTime)
{
    public required Price StartingPrice { get; init; }
    public required Price PriceDecrementRate { get; init; }
    public required TimeSpan PriceDecrementFrequency { get; init; }
    
    public static DutchAuction Restore(
        AuctionId id,
        AuctionDescription description,
        User seller,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        IAddOnlyCollection<Bid> bids,
        Price startingPrice,
        Price priceDecrementRate,
        TimeSpan priceDecrementFrequency
    ) => new (startTime, endTime)
    {
        Id = id,
        Description = description,
        Seller = seller,
        Bids = bids,
        StartingPrice = startingPrice,
        PriceDecrementRate = priceDecrementRate, 
        PriceDecrementFrequency = priceDecrementFrequency
    };
}

public sealed record FirstPriceSealedBidAuction(DateTimeOffset StartTime, DateTimeOffset EndTime)
    : Auction(StartTime, EndTime)
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
}