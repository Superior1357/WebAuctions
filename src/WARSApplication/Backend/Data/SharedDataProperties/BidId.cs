namespace Backend.Data.SharedDataProperties;

public readonly record struct BidId(int Id)
{
    public static implicit operator BidId(int id) => new BidId(id);
}