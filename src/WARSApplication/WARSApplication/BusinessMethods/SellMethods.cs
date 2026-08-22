using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WARSApplication.Data.APIData.Incoming;
using WARSApplication.Data.APIData.Outgoing;
using WARSApplication.Data.DataModel.Business;
using WARSApplication.Data.SharedDataProperties;
using WARSApplication.ErrorResponses;
using WARSApplication.ErrorResponses.SellErrorResponses;
using WARSApplication.Repository;
using WARSApplication.Utils;

namespace WARSApplication.BusinessMethods;

public static class SellMethods
{
    public static async Task<Results<Ok, BadRequest<InvalidAuctionParameters>>> CreateAuctionAsync(User user, CreatedAuction auctionApi, ICurrentTime currentTime, IRepository<Auction> repository)
    {
        throw new NotImplementedException();
        /*
        var auction = ApiToDbMapper.CreateAuction(auctionApi, user);
        await repository.AddAsync(auction);
        await repository.CommitAsync();

        return TypedResults.Ok();
        */
    }
    
    public static Task<Results<Ok, NotFound<AuctionIdNotFound>, BadRequest<InvalidAuctionParameters>>> UpdateAuctionAsync(User user, AuctionUpdateRequest r, ICurrentTime currentTime, IRepository<Auction> repository) => throw new NotImplementedException();
    public static Task<Ok<IEnumerable<AuctionId>>> ListAsync(User user, [FromRoute] AuctionStatus status, IRepository<Auction> repository) => throw new NotImplementedException();
    public static Task<Results<Ok<IEnumerable<SellerAuctionInfo>>, NotFound<AuctionIdNotFound>>> GetInfoAsync(User user, IEnumerable<AuctionId> auctionIds, IRepository<Auction> repository) => throw new NotImplementedException();
} 