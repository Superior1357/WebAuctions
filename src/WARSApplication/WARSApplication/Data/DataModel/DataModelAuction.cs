using System.ComponentModel.DataAnnotations;
using WARSApplication.Data.SharedDataProperties;

namespace WARSApplication.Data.DataModel;

public record DataModelAuction
{
    public AuctionId Id { get; init; }
    public required AuctionDescription Description { get; init; }
    public required DateTimeOffset StartTime { get; init; }
    public required DateTimeOffset EndTime { get; set; }
    public required DataModelUser Seller { get; init; }
    public required AuctionTypeSpecificRules Rules { get; init; } // TODO: remove this, make DataModelAuction abstract and introduce new types that contain the corresponding fields, then use the TPH pattern
    [ConcurrencyCheck]
    public BidId? LastPlacedBid { get; set; }
    public ICollection<DataModelBid> Bids { get; set; } = new List<DataModelBid>();
}