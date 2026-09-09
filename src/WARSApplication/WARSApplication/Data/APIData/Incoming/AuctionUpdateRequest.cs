using WARSApplication.Data.SharedDataProperties;

namespace WARSApplication.Data.APIData.Incoming;
/// <summary>
/// Request for updating an auction
/// </summary>
/// <param name="Id">Updated auction id</param>
/// <param name="NewEnd">Updated auction new end</param>
public record AuctionUpdateRequest(AuctionId Id, DateTimeOffset NewEnd);