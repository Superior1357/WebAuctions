namespace WARSApplication.Data.DataModel.Business;

public record Bid
{
    public int Id { get; init; }

    public required User Bidder { get; init; }
    
    public decimal Amount { get; init; }
    
    public DateTime Time { get; init; }
}