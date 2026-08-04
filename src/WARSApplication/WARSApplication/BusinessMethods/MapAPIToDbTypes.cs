using WARSApplication.Data.APIData.Incoming;
using WARSApplication.Data.DataModel.Business;
using WARSApplication.Data.SharedDataProperties;

namespace WARSApplication.BusinessMethods;

public static class ApiToDbMapper
{
    private static (AuctionType, decimal?) MapAuctionRule(AuctionRule rule)
    {
        return rule switch
        {
            DutchAuctionRule dutchRule => (AuctionType.Dutch, dutchRule.StartingPrice),
            EnglishAuctionRule englishRule => (AuctionType.English, englishRule.StartingPrice),
            FirstTimeSealedBidRule => (AuctionType.FirstTimeSealedBid, null),
            _ => throw new NotImplementedException()
        };
    }
    public static Auction CreateAuction(CreatedAuction auctionApi, User usr)
    {
        var mappedRule = MapAuctionRule(auctionApi.Rule);
        
        return new Auction
        {
            Seller = usr,
            StartTime = auctionApi.StartTime,
            EndTime = auctionApi.EndTime,
            Description = auctionApi.Description,
            Type = mappedRule.Item1,
            InitialPrice = mappedRule.Item2
        };
    }
    
}