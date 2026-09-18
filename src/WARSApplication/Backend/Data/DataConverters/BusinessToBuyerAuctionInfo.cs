using Backend.AuctionEvaluation;
using Backend.Data.APIData.Outgoing;
using Backend.Data.BusinessModel.Business;

namespace Backend.Data.DataConverters;

/// <summary>
/// Data converter for auctions to buyer auction info
/// </summary>
/// <param name="auctionEvaluator"></param>
internal class BusinessToBuyerAuctionInfo(IAuctionEvaluator auctionEvaluator) : IDataConverter<Auction, BuyerAuctionInfo>
{
    /// <summary>
    /// Map all properties of an auction to a buyer auction info.
    /// </summary>
    /// <param name="auction"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
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