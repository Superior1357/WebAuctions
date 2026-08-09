using Microsoft.AspNetCore.Http.HttpResults;
using WARSApplication.Data.APIData.Outgoing;
using WARSApplication.Data.DataModel.Business;
using WARSApplication.Data.SharedDataProperties;

namespace WARSApplication.BusinessMethods;

public static class BuyMethods
{
    public static Task<IEnumerable<UserId>> ListAllSellersAsync() => throw new NotImplementedException();
    public static Task<IEnumerable<AuctionId>> ListParticipatedAuctionsAsync(User usr) => throw new NotImplementedException();
    public static Task<IEnumerable<AuctionId>> ListWonAuctionsAsync(User usr) => throw new NotImplementedException();
    
    public static Task<IEnumerable<AuctionId>> ListActiveAuctionsOfAUserAsync(UserId seller) => throw new NotImplementedException();
    public static Task<Ok> BidAsync() => throw new NotImplementedException();
    public static Task<IEnumerable<BuyerAuctionInfo>> GetInfoAsync(IEnumerable<AuctionId> auctionIds) => throw new NotImplementedException();
}