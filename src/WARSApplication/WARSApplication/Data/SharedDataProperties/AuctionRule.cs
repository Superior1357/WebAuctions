namespace WARSApplication.Data.SharedDataProperties;

using System.Text.Json.Serialization;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(EnglishAuctionRule), "english")]
[JsonDerivedType(typeof(DutchAuctionRule), "dutch")]
[JsonDerivedType(typeof(FirstTimeSealedBidRule), "sealed")]
public abstract record AuctionRule;

public record EnglishAuctionRule(decimal StartingPrice) : AuctionRule;
public record DutchAuctionRule(decimal StartingPrice) : AuctionRule;
public record FirstTimeSealedBidRule : AuctionRule;