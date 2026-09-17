using Backend.Data.DataModel.Business;
using Backend.Data.SharedDataProperties;
using Backend.Utils;

namespace Backend.AuctionEvaluation;
/// <summary>
/// Auction evaluation service.
/// Supports auction evaluation for English, Dutch and First Price Sealed Bid auction types.
/// </summary>
/// <param name="time">the current time service</param>
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
    
    /// <summary>
    /// Evaluate the status of an auction.
    /// All auctions are pending until the start time.
    /// All auctions are considered active after pending until finished.
    /// All auctions are considered finished after the end time.
    /// Dutch auctions are also considered finished when a bid is made.
    /// </summary>
    /// <param name="auction">the status evaluated auction</param>
    /// <returns>the evaluated auction status</returns>
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
    
    /// <summary>
    /// Evaluate the winner info of an auction.
    /// Can only be done after the auction is finished.
    /// Dutch allow for one bid only.
    /// English auctions consider the highest bidder as the winner.
    /// First Price Sealed Bid auctions consider the highest bidder as the winner.
    /// </summary>
    /// <param name="auction">The evaluated auction</param>
    /// <returns>winner info if it can be determined</returns>
    /// <exception cref="ArgumentOutOfRangeException">when a new auction type is introduced the switch expression is not altered</exception>
    public WinnerInfo? EvaluateWinnerInfo(Auction auction)
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
    
    /// <summary>
    /// Evaluate the current minimum bid value of an auction based on its rules and status.
    /// Returns null when the auction is finished or when the minimum bid value cannot be revealed (First price sealed bid)
    /// English auctions consider the last bid as the minimum bid (or the starting price if no bid has been made yet).
    /// Dutch auctions consider the minimum bid based on current time and price decrement rate.
    /// </summary>
    /// <param name="auction">The evaluated auction.</param>
    /// <returns>The minimum bid value if it can be determined otherwise null.</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
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