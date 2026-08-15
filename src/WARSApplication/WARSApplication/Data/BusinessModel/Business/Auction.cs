using WARSApplication.Data.SharedDataProperties;

namespace WARSApplication.Data.DataModel.Business;

public record Auction
{
    public AuctionId Id { get; init; }
    public string Description { get; init; } = string.Empty;
    public required User Seller { get; init; }  
    public required DateTimeOffset StartTime { get; init; }
    public required DateTimeOffset EndTime { get; set; }
    public required AuctionTypeSpecificRules Rules { get; init; }
    public IEnumerable<Bid> GetBids() => throw new NotImplementedException();
    public AuctionStatus Status { get; init; } = AuctionStatus.Pending;
}