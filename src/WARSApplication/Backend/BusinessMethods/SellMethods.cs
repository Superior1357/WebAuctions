using Backend.AuctionEvaluation;
using Backend.Data;
using Backend.Data.APIData.Incoming;
using Backend.Data.APIData.Outgoing;
using Backend.Data.DataModel.Business;
using Backend.Data.SharedDataProperties;
using Backend.ErrorResponses;
using Backend.ErrorResponses.SellErrorResponses;
using Backend.Repository;
using Backend.Utils;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Backend.BusinessMethods;

internal static class SellMethods
{
    private static Auction ApiToBusinessAuction(User user, CreatedAuction createdAuction)
    {
        return createdAuction switch
        {
            EnglishCreatedAuction englishCreatedAuction => new
                EnglishAuction(englishCreatedAuction.StartTime, englishCreatedAuction.EndTime)
                {
                    Description = englishCreatedAuction.Description,
                    StartingPrice = englishCreatedAuction.StartingPrice,
                    Seller = user
                },
            
            DutchCreatedAuction dutchCreatedAuction => new
                DutchAuction(dutchCreatedAuction.StartTime, dutchCreatedAuction.EndTime)
                {
                    Description = dutchCreatedAuction.Description,
                    StartingPrice = dutchCreatedAuction.StartingPrice,
                    Seller = user,
                    PriceDecrementRate = dutchCreatedAuction.PriceDecrementRate,
                    PriceDecrementFrequency = dutchCreatedAuction.PriceDecrementFrequency
                },
            
            FirstPriceSealedBidCreatedAuction firstPriceSealedBidCreatedAuction => new FirstPriceSealedBidAuction(
                firstPriceSealedBidCreatedAuction.StartTime, firstPriceSealedBidCreatedAuction.EndTime)
            {
                Description = firstPriceSealedBidCreatedAuction.Description, Seller = user
            },
            
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    /// <summary>
    /// Create an auction from specified parameters.
    /// </summary>
    /// <param name="user">an authenticated user</param>
    /// <param name="createdAuction">auction creation request</param>
    /// <param name="currentTime">current time</param>
    /// <param name="repository">the repository service</param>
    /// <returns>
    /// OK status code upon success.
    /// UnprocessableEntity code and InvalidAuctionParameters message when the request is invalid.
    /// </returns>
    public static async Task<Results<Ok, UnprocessableEntity<InvalidAuctionParameters>>> CreateAuctionAsync(User user, CreatedAuction createdAuction, ICurrentTime currentTime, IRepository<Auction> repository)
    {
        if (createdAuction.StartTime < currentTime.Now)
            return TypedResults.UnprocessableEntity(new InvalidAuctionParameters("start time"));
        
        if (createdAuction.EndTime <= createdAuction.StartTime)
            return TypedResults.UnprocessableEntity(new InvalidAuctionParameters("end time"));
        
        var auction = ApiToBusinessAuction(user, createdAuction);
        await repository.AddAsync(auction);
        await repository.CommitAsync();
        return TypedResults.Ok();
    }
    
    /// <summary>
    /// Update the end time of a not-finished-yet auction.
    /// </summary>
    /// <param name="user">authenticated user</param>
    /// <param name="request">auction update request</param>
    /// <param name="auctionEvaluator">auction evaluation service</param>
    /// <param name="repository">the repository service</param>
    /// <returns>
    /// OK status code upon successful auction update.
    /// NotFound code and AuctionIdNotFound message when the auction id does not belong to an auction.
    /// UnprocessableEntity code and InvalidAuctionParameters message when the request is invalid.
    /// </returns>
    public static async Task<Results<Ok, NotFound<AuctionIdNotFound>, UnprocessableEntity<InvalidAuctionParameters>>>
        UpdateAuctionAsync(User user, AuctionUpdateRequest request, IAuctionEvaluator auctionEvaluator, IRepository<Auction> repository)
    { 
        var auctions = await repository.ListAsync();
        var auction = auctions.FirstOrDefault(a => a.Id == request.Id);
        
        if (auction is null)
            return TypedResults.NotFound(new AuctionIdNotFound($"not found {request.Id}"));
        
        if (auction.Seller != user)
            return TypedResults.NotFound(new AuctionIdNotFound("different seller than user"));
        
        if (request.NewEnd < auction.EndTime)
            return TypedResults.UnprocessableEntity(new InvalidAuctionParameters("end time"));
        
        if (auctionEvaluator.EvaluateStatus(auction) == AuctionStatus.Finished)
        {
            return TypedResults.UnprocessableEntity(new InvalidAuctionParameters("auction finished"));
        }
        
        auction.EndTime = request.NewEnd;
        
        await repository.UpdateAsync(auction);
        await repository.CommitAsync();
        return TypedResults.Ok();
    }
    
    
    /// <summary>
    /// List ids of all auctions of a seller that are in a specified status.
    /// </summary>
    /// <param name="user">an authenticated seller</param>
    /// <param name="status">status of which auctions are listed</param>
    /// <param name="evaluator">the auction evaluation service</param>
    /// <param name="repository">the repository service</param>
    /// <returns>
    /// OK status code and enumerable of specified auction ids upon success. 
    /// </returns>
    public static async Task<Ok<IEnumerable<AuctionId>>> ListAsync(User user, [FromRoute] AuctionStatus status,
        IAuctionEvaluator evaluator, IRepository<Auction> repository)
    {
        var auctions = await repository.ListAsync();
        var specified = auctions.Where(a => evaluator.EvaluateStatus(a) == status && a.Seller == user).Select(a => a.Id);
        return TypedResults.Ok(specified);
    }
    
    
    /// <summary>
    /// Get information about auction the user (seller) instantiated.
    /// </summary>
    /// <param name="user">an authenticated user</param>
    /// <param name="auctionIds">Auctions ids of which info is requested</param>
    /// <param name="repository">The repository service</param>
    /// <param name="converter">the data converter service</param>
    /// <returns>
    /// OK status code and enumerable of specified SellerAuctionInfo upon success.
    /// NotFound code and AuctionIdNotFound message when an auction id does not belong to an auction.
    /// </returns>
    public static async Task<Results<Ok<IEnumerable<SellerAuctionInfo>>, NotFound<AuctionIdNotFound>>> GetInfoAsync(User user,
        IReadOnlyCollection<AuctionId> auctionIds, IRepository<Auction> repository, IDataConverter<Auction, SellerAuctionInfo> converter)
    {
        var auctions = await repository.ListAsync();
        var userAuctions = auctions.Where(a => a.Seller == user);
        
        var chosen = userAuctions.IntersectBy(auctionIds, a => a.Id).ToList();
        
        if (chosen.Count != auctionIds.Count)
            return TypedResults.NotFound(new AuctionIdNotFound("not all auctions found"));
        
        var sortedChosen = auctionIds.Join(chosen, a => a, i => i.Id, (_, i) => i);

        var transformedChosen = sortedChosen.Select(converter.Convert);
        return TypedResults.Ok(transformedChosen);
    }
}