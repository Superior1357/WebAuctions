using WARSApplication.Data.DataModel.Business;
using WARSApplication.Data.SharedDataProperties;

namespace WARSApplication.Data.DataModel;

internal record DataModelBid
{
    public BidId Id { get; init; }
    public AuctionId AuctionId { get; init; }
    public DataModelAuction ModelAuction { get; init; } = null!;
    public required DataModelUser Bidder { get; init; }
    public required Price Value { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
}