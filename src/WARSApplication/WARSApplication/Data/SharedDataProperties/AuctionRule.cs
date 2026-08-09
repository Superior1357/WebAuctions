namespace WARSApplication.Data.SharedDataProperties;

using System.Text.Json.Serialization;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(EnglishAuctionRule), "english")]
[JsonDerivedType(typeof(DutchAuctionRule), "dutch")]
[JsonDerivedType(typeof(FirstTimeSealedBidRule), "sealed")]
public abstract record AuctionRule
{
    public static explicit operator AuctionType(AuctionRule rule)
    {
        return rule switch
        {
            EnglishAuctionRule => AuctionType.English,
            DutchAuctionRule => AuctionType.Dutch,
            FirstTimeSealedBidRule => AuctionType.FirstTimeSealedBid,
            _ => throw new NotImplementedException()
        };
    }
}

public record EnglishAuctionRule(Price StartingPrice) : AuctionRule;
public record DutchAuctionRule(Price StartingPrice) : AuctionRule;
public record FirstTimeSealedBidRule : AuctionRule;