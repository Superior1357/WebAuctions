using Backend.Data.SharedDataProperties;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.DataModel.Business;

internal sealed class AuctionDbContext(DbContextOptions<AuctionDbContext> options) : DbContext(options)
{
    public DbSet<DataModelAuction> Auctions { get; set; }
    public DbSet<DataModelUser> Users { get; set; }
    public DbSet<DataModelBid> Bids { get; set; }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<Price>().HaveConversion<PriceConverter>();
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DataModelAuction>().
            HasMany(a => a.Bids).
            WithOne(b => b.ModelAuction).
            HasForeignKey(b => b.AuctionId).
            IsRequired();
        
        modelBuilder.Entity<DataModelAuction>().HasDiscriminator<string>("auction_type").
            HasValue<EnglishDataModelAuction>("english").
            HasValue<DutchDataModelAuction>("dutch").
            HasValue<FirstPriceSealedBidDataModelAuction>("first_time_sealed");
        
        modelBuilder.Entity<DataModelAuction>().Property(a => a.Description)
            .HasConversion(a => a.Description, d => new AuctionDescription(d));

        modelBuilder.Entity<DataModelAuction>().Property(a => a.Version).IsConcurrencyToken();
        modelBuilder.Entity<DataModelBid>().Property(bid => bid.Id).HasConversion(bid => bid.Id, value => value).ValueGeneratedOnAdd();
        
        modelBuilder.Entity<DataModelAuction>().Property(a => a.Id).HasConversion(auctionId => auctionId.Id, id => new AuctionId(id)).ValueGeneratedOnAdd();
        modelBuilder.Entity<DataModelUser>().Property(a => a.Id).HasConversion(userId => userId.Id, id => new UserId(id)).ValueGeneratedOnAdd();
    }
}