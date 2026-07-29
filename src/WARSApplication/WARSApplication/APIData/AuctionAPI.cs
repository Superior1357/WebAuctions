namespace WARSApplication.APIData;

/*
add if JSON not working
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(EnglishAuction), "english")]
[JsonDerivedType(typeof(DutchAuction), "dutch")]
[JsonDerivedType(typeof(FirstTimeSealedBid), "sealed")]
*/

public abstract record AuctionType;

public record EnglishAuction(int InitialPrice) : AuctionType;
public record DutchAuction(int InitialPrice) : AuctionType;
public record FirstTimeSealedBid : AuctionType;

public record AuctionAPI(DateTime StartTime, DateTime EndTime, AuctionType Type);

public record struct AuctionId(int Id)
{
    public static bool TryParse(string s, IFormatProvider p, out AuctionId id)
    {
        var res = int.TryParse(s, p, out var i);  
        id = new AuctionId(i);
        return res;
    }
}