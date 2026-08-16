using WARSApplication.Data.DataModel.Business;
namespace UnitTests;

public record TestingAuction(ICollection<Bid>? Bids = null) : Auction
{
    public override IEnumerable<Bid> GetBids() => Bids ?? [];
}