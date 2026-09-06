using WARSApplication.Data.DataModel.Business;
using WARSApplication.Data.SharedDataProperties;

namespace WARSApplication.AuctionEvaluation;

public interface IAuctionEvaluator
{
    public AuctionStatus EvaluateStatus(Auction auction);
    public WinnerInfo? EvaluateWinner(Auction auction);
    /// <summary>
    /// null if auction is not finished
    /// </summary>
    /// <param name="auction"></param>
    /// <returns></returns>
    public Price? EvaluateCurrentMinimumBidValue(Auction auction); 
}