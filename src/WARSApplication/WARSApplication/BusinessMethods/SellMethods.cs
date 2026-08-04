using WARSApplication.Data.APIData.Incoming;
using WARSApplication.Data.DataModel.Business;
using WARSApplication.Data.SharedDataProperties;
using WARSApplication.Repository;

namespace WARSApplication.BusinessMethods;

public static class SellMethods
{
    public static async Task<IResult> CreateAuctionAsync(User user, CreatedAuction auctionApi, IAuctionRepository repository)
    {
        var auction = ApiToDbMapper.CreateAuction(auctionApi, user);
        await repository.AddAsync(auction);
        await repository.CommitAsync();
        
        return Results.Ok();
    }
    
    public static IResult UpdateAuction(AuctionUpdateRequest r, IAuctionRepository repository) => throw new NotImplementedException();
    public static async Task<IResult> ListAuctionsAsync(UserId id, IAuctionRepository repository) => throw new NotImplementedException();
    // TODO: replace ClaimsPrincipal with user ID only (if possible) - make it bind automatically
    public static async Task<IResult> ListAsync(User user, AuctionType auctionType) => throw new NotImplementedException();
    public static Task<IResult> GetInfoAsync(IEnumerable<AuctionId> auctionIds) => throw new NotImplementedException();
} 