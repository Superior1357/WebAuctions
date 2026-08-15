using System.Text.Json.Serialization;

namespace WARSApplication.Data.SharedDataProperties;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(EnglishAuctionRules), "english")]
[JsonDerivedType(typeof(DutchAuctionRules), "dutch")]
[JsonDerivedType(typeof(FirstTimeSealedBidRules), "sealed")]
public abstract record AuctionTypeSpecificRules;

public sealed record DutchAuctionRules : AuctionTypeSpecificRules
{
    public required Price StartingPrice { get; init; }
    public required Price PriceDecrementRate { get; init; }
    public required TimeSpan PriceDecrementFrequency { get; init; }
}

public sealed record EnglishAuctionRules : AuctionTypeSpecificRules
{
    public required Price StartingPrice { get; init; }
}

public sealed record FirstTimeSealedBidRules : AuctionTypeSpecificRules;