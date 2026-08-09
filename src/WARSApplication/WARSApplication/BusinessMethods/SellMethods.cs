using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WARSApplication.Data.APIData.Incoming;
using WARSApplication.Data.APIData.Outgoing;
using WARSApplication.Data.DataModel.Business;
using WARSApplication.Data.SharedDataProperties;
using WARSApplication.ErrorResponses.SellErrorResponses;
using WARSApplication.Repository;

namespace WARSApplication.BusinessMethods;

public static class SellMethods
{
    public static async Task<Results<Ok, BadRequest<InvalidAuctionParameters>>> CreateAuctionAsync(User user, CreatedAuction auctionApi, TimeProvider timeProvider, IAuctionRepository repository)
    {
        var auction = ApiToDbMapper.CreateAuction(auctionApi, user);
        await repository.AddAsync(auction);
        await repository.CommitAsync();
        
        return TypedResults.Ok();
    }
    
    public static Task<Results<Ok, NotFound<AuctionIdNotFound>, BadRequest<InvalidAuctionParameters>>> UpdateAuctionAsync(User user, AuctionUpdateRequest r, TimeProvider timeProvider, IAuctionRepository repository) => throw new NotImplementedException();
    public static async Task<IEnumerable<AuctionId>> ListAsync(User user, [FromRoute] AuctionStatus status, IAuctionRepository repository) => throw new NotImplementedException();
    public static Task<IEnumerable<SellerAuctionInfo>> GetInfoAsync(IEnumerable<AuctionId> auctionIds) => throw new NotImplementedException();
} 