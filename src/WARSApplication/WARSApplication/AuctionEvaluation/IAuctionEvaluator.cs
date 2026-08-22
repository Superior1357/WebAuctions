using WARSApplication.Data.DataModel.Business;

namespace WARSApplication.AuctionEvaluation;

public interface IAuctionEvaluator
{
    public AuctionStatus EvaluateStatus(Auction auction);
    public User? EvaluateWinner(Auction auction);
}