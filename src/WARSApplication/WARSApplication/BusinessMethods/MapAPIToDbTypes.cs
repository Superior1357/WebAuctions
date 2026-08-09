using WARSApplication.Data.APIData.Incoming;
using WARSApplication.Data.DataModel.Business;
using WARSApplication.Data.SharedDataProperties;

namespace WARSApplication.BusinessMethods;

public static class ApiToDbMapper
{
    private static Price? RuleToPrice(AuctionRule rule)
    {
        return rule switch
        {
            EnglishAuctionRule r => r.StartingPrice,
            DutchAuctionRule r => r.StartingPrice,
            FirstTimeSealedBidRule => null,
            _ => throw new NotImplementedException()
        };
    }
    
    public static Auction CreateAuction(CreatedAuction auctionApi, User usr)
    {
        var rule = auctionApi.Rule;
        
        return new Auction
        {
            Seller = usr,
            StartTime = auctionApi.StartTime,
            EndTime = auctionApi.EndTime,
            Description = auctionApi.Description,
            Type = (AuctionType)rule,
            InitialPrice = RuleToPrice(rule)
        };
    }
    
}