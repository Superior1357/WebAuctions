using Microsoft.EntityFrameworkCore;
using WARSApplication.Data.SharedDataProperties;

namespace WARSApplication.Data.DataModel.Business;

public record Auction
{
    public AuctionId Id { get; init; }
    public required User Seller { get; init; }
    public AuctionType Type { get; init; }
    public DateTimeOffset StartTime { get; init; }
    public DateTimeOffset EndTime { get; init; }
    public Bid? WinningBid { get; init; }
    public AuctionStatus Status { get; init; }
}

public sealed class AuctionDbContext(DbContextOptions<AuctionDbContext> options) : DbContext(options)
{
    public DbSet<Auction> Auctions { get; set; }
    public DbSet<User> Users { get; set; }
}