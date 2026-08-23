using Microsoft.AspNetCore.Http.HttpResults;
using WARSApplication.AuctionEvaluation;
using WARSApplication.Data.APIData.Incoming;
using WARSApplication.Data.APIData.Outgoing;
using WARSApplication.Data.DataModel.Business;
using WARSApplication.Data.SharedDataProperties;
using WARSApplication.ErrorResponses;
using WARSApplication.ErrorResponses.BuyErrorResponses;
using WARSApplication.Repository;
using WARSApplication.Utils;

namespace WARSApplication.BusinessMethods;

public static class BuyMethods
{
    public static async Task<Ok<IEnumerable<UserId>>> ListAllSellersAsync(IRepository<Auction> repository)
    {
        var auctions = await repository.ListAsync();
        var sellerIds = auctions.Select(a => a.Seller.Id).Distinct();
        
        return TypedResults.Ok(sellerIds);
    }

    public static async Task<Ok<IEnumerable<AuctionId>>> ListParticipatedAuctionsAsync(User usr,
        IRepository<Auction> repository)
    {
        var auctions = await repository.ListAsync();
        var participated = auctions.Where(a => a.Bids.GetItems().Any(b => b.Bidder == usr));
        
        return TypedResults.Ok(participated.Select(a => a.Id));
    }

    public static async Task<Ok<IEnumerable<AuctionId>>> ListWonAuctionsAsync(User usr, IRepository<Auction> repository, IAuctionEvaluator auctionEvaluator)
    {
        throw new NotImplementedException();
    }
    
    public static Task<Results<Ok<IEnumerable<AuctionId>>, NotFound<UserIdNotFound>>> ListNotFinishedAuctionsOfASellerAsync(UserId seller, IRepository<Auction> auctionRepository, IAuctionEvaluator auctionEvaluator) => throw new NotImplementedException();
    public static Task<Results<Ok, NotFound<AuctionIdNotFound>, UnprocessableEntity<BidToInactiveAuctionResponse>>> BidAsync(User usr, BidRequest bidRequest, ICurrentTime now, IRepository<Auction> auctionRepository, IAuctionEvaluator auctionEvaluator) => throw new NotImplementedException();
    public static Task<Results<Ok<IEnumerable<BuyerAuctionInfo>>, NotFound<AuctionIdNotFound>>> GetInfoAsync(IEnumerable<AuctionId> auctionIds, IRepository<Auction> auctionRepository) => throw new NotImplementedException();
}