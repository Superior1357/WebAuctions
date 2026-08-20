using WARSApplication.Data.SharedDataProperties;

namespace WARSApplication.Data.DataModel.Business;

public abstract record Auction
{
    public AuctionId Id { get; init; }
    public string Description { get; init; } = string.Empty;
    public required User Seller { get; init; }  
    public required DateTimeOffset StartTime { get; init; }
    public abstract required DateTimeOffset EndTime { get; set; }
    public required AuctionTypeSpecificRules Rules { get; init; }
    public AuctionStatus Status { get; init; } = AuctionStatus.Pending;
    public abstract IEnumerable<Bid> GetBids();
}