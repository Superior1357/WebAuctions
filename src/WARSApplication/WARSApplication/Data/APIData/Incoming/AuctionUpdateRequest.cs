using WARSApplication.Data.SharedDataProperties;

namespace WARSApplication.Data.APIData.Incoming;

public record AuctionUpdateRequest(AuctionId Id, DateTimeOffset NewEnd);