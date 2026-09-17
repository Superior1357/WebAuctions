using Backend.Data.SharedDataProperties;

namespace Backend.Data.DataModel;

/// <summary>
/// Class for a bid in the auction.
/// There's a many-to-one relationship between a bid and an auction.
/// </summary>
internal record DataModelBid
{
    public BidId Id { get; init; }
    public AuctionId AuctionId { get; init; }
    public DataModelAuction ModelAuction { get; init; } = null!;
    public required DataModelUser Bidder { get; init; }
    public required Price Value { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
}