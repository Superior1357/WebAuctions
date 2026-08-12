using Microsoft.AspNetCore.Http.HttpResults;
using WARSApplication.Data.APIData.Incoming;
using WARSApplication.Data.APIData.Outgoing;
using WARSApplication.Data.DataModel.Business;
using WARSApplication.Data.SharedDataProperties;
using WARSApplication.ErrorResponses;
using WARSApplication.Repository;

namespace WARSApplication.BusinessMethods;

public static class BuyMethods
{
    public static Task<Ok<IEnumerable<UserId>>> ListAllSellersAsync(IAuctionRepository repository) => throw new NotImplementedException();
    public static Task<Ok<IEnumerable<AuctionId>>> ListParticipatedAuctionsAsync(User usr, IBidRepository bidRepository, IAuctionRepository auctionRepository) => throw new NotImplementedException();
    public static Task<Ok<IEnumerable<AuctionId>>> ListWonAuctionsAsync(User usr, IBidRepository bidRepository, IAuctionRepository auctionRepository) => throw new NotImplementedException();
    
    public static Task<Results<Ok<IEnumerable<AuctionId>>, NotFound<UserIdNotFound>>> ListNotFinishedAuctionsOfASellerAsync(UserId seller, IAuctionRepository auctionRepository) => throw new NotImplementedException();
    public static Task<Results<Ok, NotFound<AuctionIdNotFound>>> BidAsync(User usr, BidRequest bidRequest, TimeProvider timeProvider, IAuctionRepository auctionRepository, IBidRepository bidRepository) => throw new NotImplementedException();
    public static Task<Results<Ok<IEnumerable<BuyerAuctionInfo>>, NotFound<AuctionIdNotFound>>> GetInfoAsync(IEnumerable<AuctionId> auctionIds) => throw new NotImplementedException();
}