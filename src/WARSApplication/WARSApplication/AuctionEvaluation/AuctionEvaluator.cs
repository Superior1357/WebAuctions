using WARSApplication.Data.DataModel.Business;
using WARSApplication.Data.SharedDataProperties;
using WARSApplication.Utils;

namespace WARSApplication.AuctionEvaluation;

internal class AuctionEvaluator(ICurrentTime time) : IAuctionEvaluator
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

    public WinnerInfo? EvaluateWinner(Auction auction)
    {
        if (EvaluateStatus(auction) != AuctionStatus.Finished)
            return null;

        Bid? winningBid;
        
        switch (auction)
        {
            case EnglishAuction englishAuction:
                winningBid = englishAuction.Bids.MaxBy(b => b.Value.Value);
                break;
            case DutchAuction dutchAuction:
                winningBid = dutchAuction.Bid;
                break;
            case FirstPriceSealedBidAuction firstPriceSealedBidAuction:
                winningBid = firstPriceSealedBidAuction.Bids.MaxBy(b => b.Value.Value);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(auction));
        }

        return winningBid is not null ? new WinnerInfo(winningBid.Value, winningBid.Bidder.Id) : null;
    }

    private Price CalculateActiveDutchAuctionPrice(DutchAuction auction)
    {
        var temporalDifference = time.Now - auction.StartTime;
        var passedRounds = (int)temporalDifference.Divide(auction.PriceDecrementFrequency);
        return auction.StartingPrice - passedRounds * auction.PriceDecrementRate;
    }
    
    public Price? EvaluateCurrentMinimumBidValue(Auction auction)
    {
        var status = EvaluateStatus(auction);
        if (status == AuctionStatus.Finished)
            return null;
        
        switch (auction) {
            case EnglishAuction englishAuction:
                if (status == AuctionStatus.Pending)
                    return englishAuction.StartingPrice;
                
                // status == AuctionStatus.Active

                var lastBid = englishAuction.Bids.MaxBy(b => b.Value.Value)?.Value;
                return lastBid is not null ? new Price(lastBid.Value.Value + 1) : englishAuction.StartingPrice;
            case DutchAuction dutchAuction:
                if (status == AuctionStatus.Pending)
                    return dutchAuction.StartingPrice;
                
                return CalculateActiveDutchAuctionPrice(dutchAuction);
            case FirstPriceSealedBidAuction firstPriceSealedBidAuction:
                return null;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}