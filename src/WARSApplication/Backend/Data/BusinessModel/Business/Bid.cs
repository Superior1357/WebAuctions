using Backend.Data.SharedDataProperties;

namespace Backend.Data.DataModel.Business;

/// <summary>
/// Type representing a business bid. When placing a new bid, the default constructor should be used.
/// The restore function should be used to restore an object that has been persisted, for instance, to the database.
/// </summary>
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