namespace WARSApplication.ErrorResponses.SellErrorResponses;

public sealed record InvalidAuctionParameters(string Message) : ErrorResponse(Message);