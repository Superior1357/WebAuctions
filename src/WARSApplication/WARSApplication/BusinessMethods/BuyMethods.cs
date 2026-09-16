using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WARSApplication.AuctionEvaluation;
using WARSApplication.Data;
using WARSApplication.Data.APIData.Incoming;
using WARSApplication.Data.APIData.Outgoing;
using WARSApplication.Data.DataModel.Business;
using WARSApplication.Data.SharedDataProperties;
using WARSApplication.ErrorResponses;
using WARSApplication.ErrorResponses.BuyErrorResponses;
using WARSApplication.Exceptions;
using WARSApplication.Repository;
using WARSApplication.Utils;

namespace WARSApplication.BusinessMethods;

internal static class BuyMethods
{
    /// <summary>
    /// Returns ids of all sellers of all auctions.
    /// A seller is considered such a user that instantiated at least one auction.
    /// </summary>
    /// <param name="repository">the repository service</param>
    /// <returns>OK status code and enumerable of seller user ids.</returns>
    public static async Task<Ok<IEnumerable<UserId>>> ListAllSellersAsync(IRepository<Auction> repository)
    {
        var auctions = await repository.ListAsync();
        var sellerIds = auctions.Select(a => a.Seller.Id).Distinct();
        
        return TypedResults.Ok(sellerIds);
    }
    
    /// <summary>
    /// Returns ids of all auctions in which the user participated (made a bid).
    /// </summary>
    /// <param name="usr">an authenticated user</param>
    /// <param name="repository">the repository service</param>
    /// <returns>OK status code and enumerable of auctions ids in which the user participated.</returns>
    /// <exception cref="ArgumentOutOfRangeException">new auction type has been added and the switch expression not updated</exception>
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

    /// <summary>
    /// Lists ids of all auctions in which the user won.
    /// Whether the user won is determined by the auction evaluator.
    /// </summary>
    /// <param name="usr">an authenticated user</param>
    /// <param name="repository">the repository service</param>
    /// <param name="auctionEvaluator">the auction evaluator service</param>
    /// <returns>OK status code and enumerable of auction ids corresponding to auction that the user has won.</returns>
    public static async Task<Ok<IEnumerable<AuctionId>>> ListWonAuctionsAsync(User usr, IRepository<Auction> repository, IAuctionEvaluator auctionEvaluator)
    {
        var auctions = await repository.ListAsync();
        var wonIds = auctions
            .Where(
                a =>
                {
                    var winnerEvaluation = auctionEvaluator.EvaluateWinnerInfo(a);
                    return winnerEvaluation.HasValue && winnerEvaluation.Value.WinningBidderId == usr.Id;
                }
            )
            .Select(a => a.Id);
        
        return TypedResults.Ok(wonIds);
    }
    /// <summary>
    /// List ids of all auctions of a seller that are not finished yet.
    /// Auction status is determined by the auction evaluator.
    /// </summary>
    /// <param name="seller">the seller whose auctions are requested.</param>
    /// <param name="auctionRepository">the repository service</param>
    /// <param name="auctionEvaluator">the auction evaluation service</param>
    /// <returns>OK status code and enumerable of auction ids upon success.
    /// NotFound code and UserIdNotFound message when the seller id does not belong to a seller.</returns>
    public static async Task<Results<Ok<IEnumerable<AuctionId>>, NotFound<UserIdNotFound>>>
        ListNotFinishedAuctionsOfASellerAsync([FromBody] UserId seller, IRepository<Auction> auctionRepository,
            IAuctionEvaluator auctionEvaluator)
    {
        var auctions = await auctionRepository.ListAsync();
        var sellersAuctions = auctions.Where(a => a.Seller.Id == seller);
        using var enumerator = sellersAuctions.GetEnumerator();
        var anyElement = enumerator.MoveNext();
        
        if (!anyElement)
            return TypedResults.NotFound(new UserIdNotFound("seller not found"));

        List<AuctionId> notFinishedIds = [];
        
        do
        {
            var current = enumerator.Current;
            if (auctionEvaluator.EvaluateStatus(current) != AuctionStatus.Finished)
            {
                notFinishedIds.Add(current.Id);
            }
        } while (enumerator.MoveNext());
        
        return TypedResults.Ok(notFinishedIds.AsEnumerable());
    }

    /// <summary>
    /// Assigns a new bid to an active auction if the auction rules allow it. 
    /// </summary>
    /// <param name="usr">an authenticated user</param>
    /// <param name="bidRequest">concrete bid information</param>
    /// <param name="now">current request time</param>
    /// <param name="auctionRepository">the repository service</param>
    /// <param name="auctionEvaluator">the evaluator service</param>
    /// <returns>
    /// OK status code upon success.
    /// NotFound code and AuctionIdNotFound message when the auction id does not belong to an auction.
    /// UnprocessableEntity code and BidAuctionRulesViolationResponse message when the bid violates the auction rules.
    /// Conflict code and ConcurrencyConflictResponse message when the bid is not assigned to the auction because of a concurrency conflict.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">a new auction type is added and not handled in the switch expression</exception>
    public static async Task<Results<
            Ok, 
            NotFound<AuctionIdNotFound>,
            UnprocessableEntity<BidAuctionRulesViolationResponse>, 
            Conflict<ConcurrencyConflictResponse>>>
        BidAsync(User usr, BidRequest bidRequest, ICurrentTime now, IRepository<Auction> auctionRepository,
            IAuctionEvaluator auctionEvaluator)
    {
        var auctions = await auctionRepository.ListAsync();
        var auction = auctions.FirstOrDefault(a => a.Id == bidRequest.AuctionId);

        if (auction is null)
        {
            return TypedResults.NotFound(new AuctionIdNotFound("auction not found"));
        }

        if (auctionEvaluator.EvaluateStatus(auction) != AuctionStatus.Active)
        {
            return TypedResults.UnprocessableEntity(new BidAuctionRulesViolationResponse("bidding to non-active auction"));
        }
        
        var minimumBid = auctionEvaluator.EvaluateCurrentMinimumBidValue(auction);
        if (minimumBid is not null && bidRequest.Amount.Value < minimumBid.Value.Value)
        {
            return TypedResults.UnprocessableEntity(new BidAuctionRulesViolationResponse("bid too low"));
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
                dutchAuction.Bid = bid;
                break;
            
            case FirstPriceSealedBidAuction firstPriceSealedBidAuction:
                if (firstPriceSealedBidAuction.Bids.Any(b => b.Bidder == usr))
                {
                    return TypedResults.UnprocessableEntity(new BidAuctionRulesViolationResponse("already bid"));
                }

                firstPriceSealedBidAuction.Bids.Add(bid);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        await auctionRepository.UpdateAsync(auction);
        var task = auctionRepository.CommitAsync();

        try
        {
            await task;
            return TypedResults.Ok();
        }
        
        catch (ConcurrencyException)
        {
            return TypedResults.Conflict(new ConcurrencyConflictResponse("a concurrent bid occured"));
        }
    }
    
    /// <summary>
    /// List buyer-relevant information about requested auctions.
    /// </summary>
    /// <param name="auctionIds">auction ids whose information is requested</param>
    /// <param name="auctionRepository">the auction repository service</param>
    /// <param name="dataConverter">the business to API data converter service</param>
    /// <returns>
    /// OK status code and enumerable of BuyerAuctionInfo upon success.
    /// NotFound code and AuctionIdNotFound message when an auction id does not belong to an auction.
    /// </returns>
    public static async Task<Results<Ok<IEnumerable<BuyerAuctionInfo>>, NotFound<AuctionIdNotFound>>> GetInfoAsync(
        IReadOnlyCollection<AuctionId> auctionIds, IRepository<Auction> auctionRepository, IDataConverter<Auction, BuyerAuctionInfo> dataConverter)
    {
        var auctions = await auctionRepository.ListAsync();
        var requestedAuctions = auctions.IntersectBy(auctionIds, a => a.Id).ToList();

        if (requestedAuctions.Count != auctionIds.Count)
        {
            return TypedResults.NotFound(new AuctionIdNotFound("not all auctions found"));
        }
        
        var info = requestedAuctions.Select(dataConverter.Convert);
        var correctOrderInfo = auctionIds.Join(info, a => a, i => i.AuctionId, (_, i) => i);
        
        return TypedResults.Ok(correctOrderInfo);
    }
}