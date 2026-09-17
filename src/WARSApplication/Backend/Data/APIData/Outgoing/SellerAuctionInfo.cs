using System.Text.Json.Serialization;
using Backend.Data.SharedDataProperties;

namespace Backend.Data.APIData.Outgoing;

/// <summary>
/// Outgoing API response for a single seller auction info request
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(EnglishSellerAuctionInfo), "english")]
[JsonDerivedType(typeof(DutchSellerAuctionInfo), "dutch")]
[JsonDerivedType(typeof(FirstPriceSealedBidSellerAuctionInfo), "sealed")]
public abstract record SellerAuctionInfo
{
    public required AuctionId Id { get; init; }
    
    public required AuctionDescription Description { get; init; }
    
    public required DateTimeOffset StartTime { get; init; }
    
    public required DateTimeOffset EndTime { get; init; }
    
    public required StatusSpecificInfo Info { get; init; }
}

/// <summary>
/// Outgoing API response for a single seller english auction info request
/// </summary>
public sealed record EnglishSellerAuctionInfo : SellerAuctionInfo
{
    public required Price StartingPrice { get; init; }   
}

/// <summary>
/// Outgoing API response for a single seller dutch auction info request
/// </summary>
public sealed record DutchSellerAuctionInfo : SellerAuctionInfo
{
    public required Price StartingPrice { get; init; }
    public required Price PriceDecrementRate { get; init; }
    public required TimeSpan PriceDecrementFrequency { get; init; }   
}

/// <summary>
/// Outgoing API response for a single seller first price sealed bid auction info request
/// </summary>
public sealed record FirstPriceSealedBidSellerAuctionInfo : SellerAuctionInfo;

/// <summary>
/// Outgoing API response for info dependent on auction status
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(PendingStatus), "pending")]
[JsonDerivedType(typeof(ActiveStatus), "active")]
[JsonDerivedType(typeof(FinishedStatus), "finished")]
public abstract record StatusSpecificInfo;

/// <summary>
/// Outgoing API response for a single seller auction info request with status "pending"
/// </summary>
public sealed record PendingStatus : StatusSpecificInfo;

/// <summary>
/// Outgoing API response for a single seller auction info request with status "active"
/// </summary>
public sealed record ActiveStatus : StatusSpecificInfo;

/// <summary>
/// Outgoing API response for a single seller auction info request with status "finished".
/// Contains the winner info.
/// </summary>
public sealed record FinishedStatus : StatusSpecificInfo
{
    public required WinnerInfo? Winner { get; init; }  
}