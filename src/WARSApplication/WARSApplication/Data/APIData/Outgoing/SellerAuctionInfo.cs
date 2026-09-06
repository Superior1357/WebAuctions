using WARSApplication.Data.SharedDataProperties;
namespace WARSApplication.Data.APIData.Outgoing;

public abstract record SellerAuctionInfo
{
    public required AuctionId Id { get; init; }
    
    public required AuctionDescription Description { get; init; }
    
    public required DateTimeOffset StartTime { get; init; }
    
    public required DateTimeOffset EndTime { get; init; }
    
    public required StatusSpecificInfo Info { get; init; }
}

public sealed record EnglishSellerAuctionInfo : SellerAuctionInfo
{
    public required Price StartingPrice { get; init; }   
}

public sealed record DutchSellerAuctionInfo : SellerAuctionInfo
{
    public required Price StartingPrice { get; init; }
    public required Price PriceDecrementRate { get; init; }
    public required TimeSpan PriceDecrementFrequency { get; init; }   
}

public sealed record FirstPriceSealedBidSellerAuctionInfo : SellerAuctionInfo;

public abstract record StatusSpecificInfo;

public sealed record PendingStatus : StatusSpecificInfo;

public sealed record ActiveStatus : StatusSpecificInfo;

public sealed record FinishedStatus : StatusSpecificInfo
{
    public required WinnerInfo? Winner { get; init; }  
}