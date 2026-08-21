using WARSApplication.Data.SharedDataProperties;

namespace WARSApplication.Data.DataModel.Business;

public record Bid
{
    public BidId Id { get; private init; }
    public required User Bidder { get; init; }
    public required Price Value { get; init; }
    public required DateTimeOffset Time { get; init; }

    public static Bid Restore(BidId id, User bidder, Price value, DateTimeOffset time) => new()
    {
        Id = id,
        Bidder = bidder,
        Value = value,
        Time = time,
    };
}