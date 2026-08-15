using WARSApplication.Data.DataModel.Business;
using WARSApplication.Data.SharedDataProperties;

namespace WARSApplication.Data.DataModel;

public record DataModelBid
{
    public BidId Id { get; init; } 
    public required DataModelAuction ModelAuction { get; init; }
    public required DataModelUser Bidder { get; init; }
    public required Price Value { get; init; }
    public required DateTimeOffset Timestamp;
}