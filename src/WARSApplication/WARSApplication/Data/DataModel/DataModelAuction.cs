using System.ComponentModel.DataAnnotations;
using WARSApplication.Data.SharedDataProperties;

namespace WARSApplication.Data.DataModel;

public abstract record DataModelAuction
{
    public AuctionId Id { get; init; }
    public required AuctionDescription Description { get; init; }
    public required DateTimeOffset StartTime { get; init; }
    public required DateTimeOffset EndTime { get; set; }
    public required DataModelUser Seller { get; init; }
    [ConcurrencyCheck]
    public BidId? LastPlacedBid { get; set; }
    public ICollection<DataModelBid> Bids { get; set; } = new List<DataModelBid>();
}

public sealed record EnglishDataModelAuction : DataModelAuction
{
    public Price StartingPrice { get; set; }
}

public sealed record DutchDataModelAuction : DataModelAuction
{
    public required Price StartingPrice { get; init; }
    public required Price PriceDecrementRate { get; init; }
    public required TimeSpan PriceDecrementFrequency { get; init; }
}

public sealed record FirstTimeSealedBidDataModelAuction : DataModelAuction;