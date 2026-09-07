using WARSApplication.AuctionEvaluation;
using WARSApplication.Data.APIData.Outgoing;
using WARSApplication.Data.DataModel.Business;

namespace WARSApplication.Data.DataConverters;

internal class AuctionToSellerAuctionInfo(IAuctionEvaluator auctionEvaluator) : IDataConverter<Auction, SellerAuctionInfo>
{
    public SellerAuctionInfo Convert(Auction entity)
    {
        var status = auctionEvaluator.EvaluateStatus(entity);
        StatusSpecificInfo statusSpecificInfo;

        switch (status)
        {
            case AuctionStatus.Active:
                statusSpecificInfo = new ActiveStatus();
                break;
            case AuctionStatus.Pending:
                statusSpecificInfo = new PendingStatus();
                break;
            case AuctionStatus.Finished:
                statusSpecificInfo = new FinishedStatus
                {
                    Winner = auctionEvaluator.EvaluateWinner(entity)
                };
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }
        
        switch (entity)
        {
            case EnglishAuction englishAuction:
                return new EnglishSellerAuctionInfo
                {
                    Description = englishAuction.Description,
                    EndTime = englishAuction.EndTime,
                    Id = englishAuction.Id,
                    Info = statusSpecificInfo,
                    StartingPrice = englishAuction.StartingPrice,
                    StartTime = englishAuction.StartTime
                };
            
            case DutchAuction dutchAuction:
                return new DutchSellerAuctionInfo
                {
                    Description = dutchAuction.Description,
                    EndTime = dutchAuction.EndTime,
                    Id = dutchAuction.Id,
                    Info = statusSpecificInfo,
                    PriceDecrementFrequency = dutchAuction.PriceDecrementFrequency,
                    PriceDecrementRate = dutchAuction.PriceDecrementRate,
                    StartingPrice = dutchAuction.StartingPrice,
                    StartTime = dutchAuction.StartTime
                };
            
            case FirstPriceSealedBidAuction firstPriceSealedBidAuction:
                return new FirstPriceSealedBidSellerAuctionInfo
                {
                    Description = firstPriceSealedBidAuction.Description,
                    EndTime = firstPriceSealedBidAuction.EndTime,
                    Id = firstPriceSealedBidAuction.Id,
                    Info = statusSpecificInfo,
                    StartTime = firstPriceSealedBidAuction.StartTime
                };
            
            default:
                throw new ArgumentOutOfRangeException(nameof(entity), entity, null);
        }
        
        
    }
}