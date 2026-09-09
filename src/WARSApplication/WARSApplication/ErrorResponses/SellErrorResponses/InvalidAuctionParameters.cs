namespace WARSApplication.ErrorResponses.SellErrorResponses;

/// <summary>
/// Error response for instantiating or updating an auction with invalid parameters
/// </summary>
/// <param name="Message"></param>
public sealed record InvalidAuctionParameters(string Message) : ErrorResponse(Message);