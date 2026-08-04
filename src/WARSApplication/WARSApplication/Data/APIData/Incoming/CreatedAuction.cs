using WARSApplication.Data.SharedDataProperties;
namespace WARSApplication.Data.APIData.Incoming;

public record CreatedAuction(
        DateTimeOffset StartTime, DateTimeOffset EndTime, string Description, AuctionRule Rule
    );