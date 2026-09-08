namespace WARSApplication.ErrorResponses.BuyErrorResponses;

public record BidAuctionRulesViolationResponse(string Message) : ErrorResponse(Message);