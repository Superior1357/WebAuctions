namespace WARSApplication.ErrorResponses.SellErrorResponses;

public abstract record ErrorResponse;

public sealed record InvalidAuctionParameters : ErrorResponse;