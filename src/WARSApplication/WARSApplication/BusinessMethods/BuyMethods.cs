using WARSApplication.Data.DataModel.Business;
using WARSApplication.Data.SharedDataProperties;

namespace WARSApplication.BusinessMethods;

public static class BuyMethods
{
    public static Task<IResult> ListAllSellersAsync() => throw new NotImplementedException();
    public static Task<IResult> ListParticipatedAuctionsAsync(User usr) => throw new NotImplementedException();
    public static Task<IResult> ListWonAuctionsAsync(User usr) => throw new NotImplementedException();
    
    public static Task<IResult> ListPendingAuctionsAsync(User seller, AuctionType auctionType) => throw new NotImplementedException();
    public static Task<IResult> BidAsync() => throw new NotImplementedException();
    public static Task<IResult> GetInfoAsync(IEnumerable<AuctionId> auctionIds) => throw new NotImplementedException();
}