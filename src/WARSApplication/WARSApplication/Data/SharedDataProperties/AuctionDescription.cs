namespace WARSApplication.Data.SharedDataProperties;

public readonly record struct AuctionDescription(string Description)
{
    public static implicit operator AuctionDescription(string description) => new(description);
}