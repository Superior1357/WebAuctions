namespace Backend.ErrorResponses.BuyErrorResponses;

/// <summary>
/// Response for a bid auction rules violation.
/// Raised when a bid is placed that violates the auction rules.
/// </summary>
/// <param name="Message"></param>
public record BidAuctionRulesViolationResponse(string Message) : ErrorResponse(Message);