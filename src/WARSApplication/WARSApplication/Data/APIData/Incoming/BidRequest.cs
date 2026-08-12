using WARSApplication.Data.SharedDataProperties;

namespace WARSApplication.Data.APIData.Incoming;

public record BidRequest
{
    public required AuctionId AuctionId { get; init; }
    public required Price Amount { get; init; }
}