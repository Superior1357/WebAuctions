using Backend.Data.SharedDataProperties;

namespace Backend.Data.APIData.Incoming;
/// <summary>
/// Request for updating an auction
/// </summary>
/// <param name="Id">Updated auction id</param>
/// <param name="NewEnd">Updated auction new end</param>
public record AuctionUpdateRequest(AuctionId Id, DateTimeOffset NewEnd);