namespace WARSApplication.Data.SharedDataProperties;

public readonly record struct UserId(int Id)
{
    public static AuctionId operator+(UserId id, int n) => new (id.Id + n);
}