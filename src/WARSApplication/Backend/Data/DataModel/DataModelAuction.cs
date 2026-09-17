using System.ComponentModel.DataAnnotations;
using Backend.Data.SharedDataProperties;

namespace Backend.Data.DataModel;

/// <summary>
/// Base class for all data model auctions.
/// </summary>
internal abstract record DataModelAuction
{
    public AuctionId Id { get; init; }
    public required AuctionDescription Description { get; init; }
    public required DateTimeOffset StartTime { get; init; }
    public required DateTimeOffset EndTime { get; set; }
    public required DataModelUser Seller { get; init; }
    
    [ConcurrencyCheck]
    public int Version { get; private set; }
    public void IncrementVersion() => Version++; // only when changing bids since end time is prolonged only 
    public ICollection<DataModelBid> Bids { get; init; } = new List<DataModelBid>();
}

internal sealed record EnglishDataModelAuction : DataModelAuction
{
    public Price StartingPrice { get; init; }
}

internal sealed record DutchDataModelAuction : DataModelAuction
{
    public required Price StartingPrice { get; init; }
    public required Price PriceDecrementRate { get; init; }
    public required TimeSpan PriceDecrementFrequency { get; init; }
}

internal sealed record FirstPriceSealedBidDataModelAuction : DataModelAuction;