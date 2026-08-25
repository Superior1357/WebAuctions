using System.Text.Json.Serialization;
using WARSApplication.Data.SharedDataProperties;

namespace WARSApplication.Data.APIData.Incoming;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(EnglishCreatedAuction), "english")]
[JsonDerivedType(typeof(DutchCreatedAuction), "dutch")]
[JsonDerivedType(typeof(FirstPriceSealedBidCreatedAuction), "sealed")]
public abstract record CreatedAuction()
{
    public required DateTimeOffset StartTime { get; init; }
    public required DateTimeOffset EndTime { get; init; }
    public required string Description { get; init; }
}

public sealed record EnglishCreatedAuction : CreatedAuction
{
    public required Price StartingPrice { get; init; }    
}

public sealed record DutchCreatedAuction : CreatedAuction
{
    public required Price StartingPrice { get; init; }
    public required Price PriceDecrementRate { get; init; }
    public required TimeSpan PriceDecrementFrequency { get; init; }
}

public sealed record FirstPriceSealedBidCreatedAuction : CreatedAuction;