using WARSApplication.AuctionEvaluation;
using WARSApplication.Data.APIData.Outgoing;
using WARSApplication.Data.DataModel.Business;

namespace WARSApplication.Data.DataConverters;

public class AuctionToSellerAuctionInfo(IAuctionEvaluator auctionEvaluator) : IDataConverter<Auction, SellerAuctionInfo>
{
    public SellerAuctionInfo Convert(Auction entity)
    {
        var status = auctionEvaluator.EvaluateStatus(entity);
        StatusSpecificInfo statusSpecificInfo;
        
        if (status == AuctionStatus.Finished)
        {
            var winner = auctionEvaluator.EvaluateWinner(entity);
        }
        
        switch (entity)
        {
            case EnglishAuction englishAuction:
                throw new NotImplementedException();
            case DutchAuction dutchAuction:
                throw new NotImplementedException();
            case FirstPriceSealedBidAuction firstPriceSealedBidAuction:
                throw new NotImplementedException();
            default:
                throw new ArgumentOutOfRangeException(nameof(entity), entity, null);
        }
        
        
    }
}