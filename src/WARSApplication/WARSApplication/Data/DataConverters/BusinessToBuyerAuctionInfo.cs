using WARSApplication.AuctionEvaluation;
using WARSApplication.Data.APIData.Outgoing;
using WARSApplication.Data.DataModel.Business;

namespace WARSApplication.Data.DataConverters;

// TODO: tests for this class/service
internal class BusinessToBuyerAuctionInfo(IAuctionEvaluator auctionEvaluator) : IDataConverter<Auction, BuyerAuctionInfo>
{
    public BuyerAuctionInfo Convert(Auction auction)
    {
        var auctionStatus = auctionEvaluator.EvaluateStatus(auction);

        return auction switch
        {
            EnglishAuction englishAuction => new EnglishAuctionInfo
            {
                AuctionId = englishAuction.Id,
                Description = englishAuction.Description,
                EndTime = englishAuction.EndTime,
                CurrentMinimumBid = auctionEvaluator.EvaluateCurrentMinimumBidValue(englishAuction),
                SellerId = englishAuction.Seller.Id,
                StartTime = englishAuction.StartTime,
                Status = auctionStatus
            },
            
            DutchAuction dutchAuction => new DutchAuctionInfo
            {
                AuctionId = dutchAuction.Id,
                CurrentPrice = auctionEvaluator.EvaluateCurrentMinimumBidValue(dutchAuction),
                Description = dutchAuction.Description,
                EndTime = dutchAuction.EndTime,
                SellerId = dutchAuction.Seller.Id,
                StartTime = dutchAuction.StartTime,
                Status = auctionStatus
            },
            
            FirstPriceSealedBidAuction firstPriceSealedBidAuction => new FirstPriceSealedBidInfo
            {
                AuctionId = firstPriceSealedBidAuction.Id,
                Description = firstPriceSealedBidAuction.Description,
                EndTime = firstPriceSealedBidAuction.EndTime,
                SellerId = firstPriceSealedBidAuction.Seller.Id,
                StartTime = firstPriceSealedBidAuction.StartTime,
                Status = auctionStatus
            },
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}