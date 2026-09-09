using WARSApplication;
using WARSApplication.AuctionEvaluation;
using WARSApplication.Data.DataModel.Business;
using WARSApplication.Data.SharedDataProperties;

namespace UnitTests.HelperClasses;

internal class MockAuctionEvaluator(IDictionary<AuctionId, AuctionStatus> auctionStatusMap, IDictionary<AuctionId, WinnerInfo?> auctionToWinnerMap,
    IDictionary<AuctionId, Price?> auctionToMinimumBidValueMap) : IAuctionEvaluator
{
    public AuctionStatus EvaluateStatus(Auction auction) => auctionStatusMap[auction.Id];
    public WinnerInfo? EvaluateWinnerInfo(Auction auction) => auctionToWinnerMap[auction.Id];
    public Price? EvaluateCurrentMinimumBidValue(Auction auction) => auctionToMinimumBidValueMap[auction.Id];
}