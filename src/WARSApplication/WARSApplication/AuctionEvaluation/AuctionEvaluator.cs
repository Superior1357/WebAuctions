using WARSApplication.Data.DataModel.Business;
using WARSApplication.Data.SharedDataProperties;
using WARSApplication.Utils;

namespace WARSApplication.AuctionEvaluation;

public class AuctionEvaluator(ICurrentTime time) : IAuctionEvaluator
{
    private static AuctionStatus EvaluateAuctionTypeSpecificActiveTimeWindowStatus(Auction auction)
    {
        switch (auction)
        {
            case EnglishAuction:
                return AuctionStatus.Active;
            case DutchAuction dutchAuction:
                return dutchAuction.Bid is not null ? AuctionStatus.Finished : AuctionStatus.Active;
            case FirstPriceSealedBidAuction:
                return AuctionStatus.Active;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    public AuctionStatus EvaluateStatus(Auction auction)
    {
        var currentTime = time.Now;
        
        if (currentTime >= auction.EndTime)
        {
            return AuctionStatus.Finished;
        }

        if (currentTime <= auction.StartTime)
        {
            return AuctionStatus.Pending;
        }
        
        return EvaluateAuctionTypeSpecificActiveTimeWindowStatus(auction);
    }

    public User? EvaluateWinner(Auction auction)
    {
        if (EvaluateStatus(auction) != AuctionStatus.Finished)
            return null;

        return auction switch
        {
            EnglishAuction englishAuction => englishAuction.Bids.MaxBy(b => b.Value.Value)?.Bidder,
            DutchAuction dutchAuction => dutchAuction.Bid?.Bidder,
            FirstPriceSealedBidAuction firstPriceSealedBidAuction => firstPriceSealedBidAuction.Bids.MaxBy(b => b.Value.Value)
                ?.Bidder,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public Price? EvaluateCurrentMinimumBidValue(Auction auction)
    {
        throw new NotImplementedException();
    }
}