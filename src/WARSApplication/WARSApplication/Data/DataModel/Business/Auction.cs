using Microsoft.EntityFrameworkCore;
using WARSApplication.Data.SharedDataProperties;

namespace WARSApplication.Data.DataModel.Business;

public record Auction
{
    public AuctionId Id { get; init; }
    public string Description { get; init; } = string.Empty;
    public required User Seller { get; init; }  
    public required AuctionType Type { get; init; }
    public Price? InitialPrice { get; init; }
    public required DateTimeOffset StartTime { get; init; }
    public required DateTimeOffset EndTime { get; init; }
    public BidId? WinningBid { get; set; }
    public AuctionStatus Status { get; init; } = AuctionStatus.Pending;
}

public sealed class AuctionDbContext(DbContextOptions<AuctionDbContext> options) : DbContext(options)
{
    public DbSet<Auction> Auctions { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Bid> Bids { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bid>().Property(bid => bid.Id).HasConversion(bid => bid.Id, value => value);
        modelBuilder.Entity<Bid>().Property(bid => bid.Amount).HasConversion(price => price.Value, value => new Price(value));
        modelBuilder.Entity<Auction>().Property(a => a.InitialPrice).HasConversion(price => price.HasValue ? price.Value.Value : (decimal?)null, value => value.HasValue ? new Price(value.Value) : null);
        modelBuilder.Entity<Auction>().Property(a => a.Id).HasConversion(auctionId => auctionId.Id, id => new AuctionId(id)).ValueGeneratedOnAdd();
        modelBuilder.Entity<Auction>().Property(a => a.WinningBid).HasConversion(bid => bid.HasValue ? bid.Value.Id : (int?)null, value => value);
        modelBuilder.Entity<User>().Property(a => a.Id).HasConversion(userId => userId.Id, id => new UserId(id)).ValueGeneratedOnAdd();
    }
}