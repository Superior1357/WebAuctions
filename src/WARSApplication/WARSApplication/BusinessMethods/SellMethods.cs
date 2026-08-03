using System.Security.Claims;
using WARSApplication.Data.APIData.Incoming;
using WARSApplication.Data.SharedDataProperties;
using WARSApplication.Repository;

namespace WARSApplication.BusinessMethods;

public static class SellMethods
{
    public static async Task<IResult> CreateAuctionAsync(CreatedAuction auctionApi, IAuctionRepository repository) =>
        throw new NotImplementedException();
    public static IResult UpdateAuction(AuctionUpdateRequest r, IAuctionRepository repository) => throw new NotImplementedException();
    public static async Task<IResult> ListAuctionsAsync(UserId id, IAuctionRepository repository) => throw new NotImplementedException();
    // TODO: replace ClaimsPrincipal with user ID only (if possible) - make it bind automatically
    public static async Task<IResult> ListAsync(UserId id, AuctionType auctionType) => throw new NotImplementedException();
    public static Task<IResult> GetInfoAsync(IEnumerable<AuctionId> auctionIds) => throw new NotImplementedException();
} 