namespace WARSApplication.Data.SharedDataProperties;
/// <summary>
/// Information about the winner of an auction
/// </summary>
/// <param name="WinningBid">the winning bid in an auction</param>
/// <param name="WinningBidderId">the winner bidder id</param>
public readonly record struct WinnerInfo(Price WinningBid, UserId WinningBidderId);