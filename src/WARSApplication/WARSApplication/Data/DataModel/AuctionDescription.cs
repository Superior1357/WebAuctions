namespace WARSApplication.Data.DataModel;

public readonly record struct AuctionDescription(string Description)
{
    public static implicit operator AuctionDescription(string description) => new(description);
}