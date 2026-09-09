using WARSApplication.Data.SharedDataProperties;

namespace WARSApplication.Data.APIData.Incoming;

/// <summary>
/// API request for placing a bid
/// </summary>
/// <param name="AuctionId"></param>
/// <param name="Amount"></param>
public record BidRequest(AuctionId AuctionId, Price Amount);