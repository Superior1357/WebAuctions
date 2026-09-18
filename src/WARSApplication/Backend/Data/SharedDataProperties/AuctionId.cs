namespace Backend.Data.SharedDataProperties;

public readonly record struct AuctionId(int Id)
{
    public static AuctionId operator+(AuctionId id, int n) => new (id.Id + n);
}