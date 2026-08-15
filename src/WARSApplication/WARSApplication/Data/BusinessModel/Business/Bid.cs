using WARSApplication.Data.SharedDataProperties;

namespace WARSApplication.Data.DataModel.Business;

public record Bid
{
    public BidId Id { get; init; }
    public required User Bidder { get; init; }
    public required Price Value { get; init; }
    public required DateTimeOffset Time { get; init; }
}