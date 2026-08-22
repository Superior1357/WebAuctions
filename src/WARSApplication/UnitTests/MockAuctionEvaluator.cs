using WARSApplication;
using WARSApplication.AuctionEvaluation;
using WARSApplication.Data.DataModel.Business;
using WARSApplication.Data.SharedDataProperties;

namespace UnitTests;

public class MockAuctionEvaluator(IDictionary<AuctionId, AuctionStatus> auctionStatusMap, IDictionary<AuctionId, User?> auctionToWinnerMap) : IAuctionEvaluator
{
    public AuctionStatus EvaluateStatus(Auction auction) => auctionStatusMap[auction.Id];
    public User? EvaluateWinner(Auction auction) => auctionToWinnerMap[auction.Id];
}