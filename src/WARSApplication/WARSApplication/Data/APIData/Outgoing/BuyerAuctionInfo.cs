using WARSApplication.Data.SharedDataProperties;

namespace WARSApplication.Data.APIData.Outgoing;

public abstract record BuyerAuctionInfo
{
    public required AuctionId AuctionId { get; init; }
    public required AuctionDescription Description { get; init; }
    public required UserId SellerId { get; init; }
    public required DateTimeOffset StartTime { get; init; }
    public required DateTimeOffset EndTime { get; init; }
    public required AuctionStatus Status { get; init; }
}

public sealed record DutchAuctionInfo : BuyerAuctionInfo
{
    public required Price? CurrentPrice { get; init; }
}

public sealed record EnglishAuctionInfo : BuyerAuctionInfo
{
    public required Price? CurrentMinimumBid { get; init; }   
}
public sealed record FirstPriceSealedBidInfo : BuyerAuctionInfo;