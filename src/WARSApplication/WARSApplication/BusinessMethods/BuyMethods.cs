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
        var participated = auctions.Where(auction =>
        {
            return auction switch
            {
                EnglishAuction a => a.Bids.Any(b => b.Bidder == usr),
                FirstPriceSealedBidAuction a => a.Bids.Any(b => b.Bidder == usr),
                DutchAuction a => a.Bid is not null && a.Bid.Bidder == usr,
                _ => throw new ArgumentOutOfRangeException()
            };
        });
        
        return TypedResults.Ok(participated.Select(a => a.Id));
    }

    public static async Task<Ok<IEnumerable<AuctionId>>> ListWonAuctionsAsync(User usr, IRepository<Auction> repository, IAuctionEvaluator auctionEvaluator)
    {
        var auctions = await repository.ListAsync();
        var wonIds = auctions.Where(a => auctionEvaluator.EvaluateWinner(a) == usr).Select(a => a.Id);
        
        return TypedResults.Ok(wonIds);
    }

    public static async Task<Results<Ok<IEnumerable<AuctionId>>, NotFound<UserIdNotFound>>>
        ListNotFinishedAuctionsOfASellerAsync(UserId seller, IRepository<Auction> auctionRepository,
            IAuctionEvaluator auctionEvaluator) // TODO: checks somewhere else? - e.g add a foreign user?
    {
        var auctions = await auctionRepository.ListAsync();
        var notFinished = auctions.Where(a => auctionEvaluator.EvaluateStatus(a) != AuctionStatus.Finished && a.Seller.Id == seller);
        
        return TypedResults.Ok(notFinished.Select(a => a.Id));
    }

    public static async Task<Results<Ok, NotFound<AuctionIdNotFound>, UnprocessableEntity<BidAuctionRulesViolationResponse>>>
        BidAsync(User usr, BidRequest bidRequest, ICurrentTime now, IRepository<Auction> auctionRepository,
            IAuctionEvaluator auctionEvaluator)
    {
        var auctions = await auctionRepository.ListAsync();
        var auction = auctions.FirstOrDefault(a => a.Id == bidRequest.AuctionId);

        if (auction is null)
        {
            return TypedResults.NotFound(new AuctionIdNotFound());
        }
        
        var bid = new Bid
        {
            Bidder = usr,
            Time = now.Now,
            Value = bidRequest.Amount
        };
        
        switch (auction)
        {
            case EnglishAuction englishAuction:
                englishAuction.Bids.Add(bid);
                break;
            case DutchAuction dutchAuction:
                if (dutchAuction.Bid is not null)
                    dutchAuction.Bid = bid;
                break;
            case FirstPriceSealedBidAuction firstPriceSealedBidAuction:
                firstPriceSealedBidAuction.Bids.Add(bid);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        await auctionRepository.UpdateAsync(auction);
        await auctionRepository.CommitAsync();

        return TypedResults.Ok();
    }

    public static Task<Results<Ok<IEnumerable<BuyerAuctionInfo>>, NotFound<AuctionIdNotFound>>> GetInfoAsync(
        IEnumerable<AuctionId> auctionIds, IRepository<Auction> auctionRepository)
    {
        throw new NotImplementedException(); // TODO: change BuyerAuctionInfo API
    }
}