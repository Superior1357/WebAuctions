using Microsoft.EntityFrameworkCore;
using WARSApplication.Data.SharedDataProperties;

namespace WARSApplication.Data.DataModel.Business;

public record Auction
{
    public AuctionId Id { get; init; }
    public string Description { get; init; } = string.Empty;
    public required User Seller { get; init; }
    public required AuctionType Type { get; init; }
    public decimal? InitialPrice { get; init; }
    public required DateTimeOffset StartTime { get; init; }
    public required DateTimeOffset EndTime { get; init; }
    public Bid? WinningBid { get; init; }
    public AuctionStatus Status { get; init; } = AuctionStatus.Pending;
}

public sealed class AuctionDbContext(DbContextOptions<AuctionDbContext> options) : DbContext(options)
{
    public DbSet<Auction> Auctions { get; set; }
    public DbSet<User> Users { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Auction>().Property(a => a.Id).HasConversion(auctionId => auctionId.Id, id => new AuctionId(id));
        modelBuilder.Entity<User>().Property(a => a.Id).HasConversion(userId => userId.Id, id => new UserId(id));
    }
}