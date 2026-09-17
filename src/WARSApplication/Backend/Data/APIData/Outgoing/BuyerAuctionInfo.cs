using System.Text.Json.Serialization;
using Backend.Data.SharedDataProperties;

namespace Backend.Data.APIData.Outgoing;

/// <summary>
/// Outgoing API response for a single buyer auction info request
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(EnglishAuctionInfo), "english")]
[JsonDerivedType(typeof(DutchAuctionInfo), "dutch")]
[JsonDerivedType(typeof(FirstPriceSealedBidInfo), "sealed")]
public abstract record BuyerAuctionInfo
{
    public required AuctionId AuctionId { get; init; }
    public required AuctionDescription Description { get; init; }
    public required UserId SellerId { get; init; }
    public required DateTimeOffset StartTime { get; init; }
    public required DateTimeOffset EndTime { get; init; }
    public required AuctionStatus Status { get; init; }
}

/// <summary>
/// Outgoing API response for a single dutch buyer auction info request
/// </summary>
public sealed record DutchAuctionInfo : BuyerAuctionInfo
{
    public required Price? CurrentPrice { get; init; }
}

/// <summary>
/// Outgoing API response for a single dutch buyer auction info request
/// </summary>
public sealed record EnglishAuctionInfo : BuyerAuctionInfo
{
    public required Price? CurrentMinimumBid { get; init; }   
}

/// <summary>
/// Outgoing API response for a single dutch buyer auction info request
/// </summary>
public sealed record FirstPriceSealedBidInfo : BuyerAuctionInfo;