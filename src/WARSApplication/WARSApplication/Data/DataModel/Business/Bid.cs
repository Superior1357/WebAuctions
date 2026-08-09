using WARSApplication.Data.SharedDataProperties;

namespace WARSApplication.Data.DataModel.Business;

public record Bid
{
    public int Id { get; init; }
    public required User Bidder { get; init; }
    public required Price Amount { get; init; }
    public required DateTimeOffset Time { get; init; }
}