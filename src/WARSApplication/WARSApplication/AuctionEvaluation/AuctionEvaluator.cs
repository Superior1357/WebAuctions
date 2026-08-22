using WARSApplication.Data.DataModel.Business;
using WARSApplication.Utils;

namespace WARSApplication.AuctionEvaluation;

public class AuctionEvaluator(ICurrentTime time) : IAuctionEvaluator
{
    public AuctionStatus EvaluateStatus(Auction auction)
    {
        throw new NotImplementedException();
    }

    public User? EvaluateWinner(Auction auction)
    {
        throw new NotImplementedException();
    }
}

// TODO: test this class (reuse the tests from the repository)