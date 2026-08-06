using WARSApplication.Data.SharedDataProperties;

namespace WARSApplication.Data.APIData.Incoming;

public record Bid
{
    public required AuctionId AuctionId { get; init; }
    public required decimal Amount { get; init; }
}