namespace WARSApplication.DataModel;
using WARSApplication.APIData;
using Microsoft.EntityFrameworkCore;

public class Auction
{
    public int Id { get; set; }
    
    public string Owner { get; set; }
}

public sealed class AuctionDbContext(DbContextOptions<AuctionDbContext> options) : DbContext(options)
{
    public DbSet<Auction> Auctions { get; set; }
}