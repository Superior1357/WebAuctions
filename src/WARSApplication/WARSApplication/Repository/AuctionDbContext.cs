using Microsoft.EntityFrameworkCore;
using WARSApplication.Data.SharedDataProperties;

namespace WARSApplication.Data.DataModel.Business;

public sealed class AuctionDbContext(DbContextOptions<AuctionDbContext> options) : DbContext(options)
{
    public DbSet<DataModelAuction> Auctions { get; set; }
    public DbSet<DataModelUser> Users { get; set; }
    public DbSet<DataModelBid> Bids { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DataModelAuction>().Property(a => a.Description)
            .HasConversion(a => a.Description, d => new AuctionDescription(d));
        
        modelBuilder.Entity<DataModelBid>().Property(bid => bid.Id).HasConversion(bid => bid.Id, value => value);
        modelBuilder.Entity<DataModelBid>().Property(bid => bid.Value).HasConversion(bid => bid.Value, value => new Price(value));
        
        modelBuilder.Entity<DataModelAuction>().Property(a => a.Id).HasConversion(auctionId => auctionId.Id, id => new AuctionId(id)).ValueGeneratedOnAdd();
        modelBuilder.Entity<DataModelUser>().Property(a => a.Id).HasConversion(userId => userId.Id, id => new UserId(id)).ValueGeneratedOnAdd();
    }
}