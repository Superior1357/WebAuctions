using WARSApplication.Data.SharedDataProperties;

namespace WARSApplication.BusinessMethods;

public static class BuyMethods
{
    public static Task<IResult> ListAllSellersAsync() => throw new NotImplementedException();
    public static Task<IResult> ListParticipatedAuctionsAsync(UserId id) => throw new NotImplementedException();
    public static Task<IResult> ListWonAuctionsAsync(UserId id) => throw new NotImplementedException();
    
    public static Task<IResult> ListPendingAuctionsAsync(UserId sellerId, AuctionType auctionType) => throw new NotImplementedException();
    public static Task<IResult> BidAsync() => throw new NotImplementedException();
    public static Task<IResult> GetInfoAsync(IEnumerable<AuctionId> auctionIds) => throw new NotImplementedException();
}