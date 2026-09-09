namespace WARSApplication.ErrorResponses;
/// <summary>
/// Error response for auction id not found
/// </summary>
/// <param name="Message"></param>
public record AuctionIdNotFound(string Message) : ErrorResponse(Message);