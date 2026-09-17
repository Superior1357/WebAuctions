using Backend.Data.DataModel.Business;
using Backend.Data.SharedDataProperties;

namespace Backend.AuctionEvaluation;
/// <summary>
/// Auction evaluation service
/// </summary>
internal interface IAuctionEvaluator
{
    /// <summary>
    /// Evaluates auction status based on auction type and specific conditions.
    /// </summary>
    /// <param name="auction">Evaluated auction</param>
    /// <returns>Current auction status</returns>
    public AuctionStatus EvaluateStatus(Auction auction);
    /// <summary>
    /// Evaluates auction winner on a finished auction based on auction type and specific conditions.
    /// </summary>
    /// <param name="auction">Evaluated auction</param>
    /// <returns>Current winner info. Null if it cannot be determined.</returns>
    public WinnerInfo? EvaluateWinnerInfo(Auction auction);
    /// <summary>
    /// Evaluates current minimum bid value on an auction based on auction type and specific conditions.
    /// </summary>
    /// <param name="auction">Evaluated auction</param>
    /// <returns>Minimum bid value. Null if it cannot be determined</returns>
    public Price? EvaluateCurrentMinimumBidValue(Auction auction); 
}