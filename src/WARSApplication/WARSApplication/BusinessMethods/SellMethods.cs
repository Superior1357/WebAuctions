using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WARSApplication.AuctionEvaluation;
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
    private static Auction ApiToBusinessAuction(User user, CreatedAuction createdAuction)
    {
        switch (createdAuction)
        {
            case EnglishCreatedAuction englishCreatedAuction:
                return new EnglishAuction(englishCreatedAuction.StartTime, englishCreatedAuction.EndTime)
                {
                    Description = englishCreatedAuction.Description,
                    StartingPrice = englishCreatedAuction.StartingPrice,
                    Seller = user
                };
            
            case DutchCreatedAuction dutchCreatedAuction:
                return new DutchAuction(dutchCreatedAuction.StartTime, dutchCreatedAuction.EndTime)
                {
                    Description = dutchCreatedAuction.Description,
                    StartingPrice = dutchCreatedAuction.StartingPrice,
                    Seller = user,
                    PriceDecrementRate = dutchCreatedAuction.PriceDecrementRate,
                    PriceDecrementFrequency = dutchCreatedAuction.PriceDecrementFrequency
                };
            case FirstPriceSealedBidCreatedAuction firstPriceSealedBidCreatedAuction:
                return new FirstPriceSealedBidAuction(firstPriceSealedBidCreatedAuction.StartTime,
                    firstPriceSealedBidCreatedAuction.EndTime)
                {
                    Description = firstPriceSealedBidCreatedAuction.Description,
                    Seller = user
                };
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public static async Task<Results<Ok, BadRequest<InvalidAuctionParameters>>> CreateAuctionAsync(User user, CreatedAuction createdAuction, ICurrentTime currentTime, IRepository<Auction> repository)
    { // TODO: why not use a factory service? (to transform between api and business) - maybe an antipattern if this used in one method only
        var auction = ApiToBusinessAuction(user, createdAuction);
        await repository.AddAsync(auction);
        await repository.CommitAsync();
        return TypedResults.Ok();
    }

    public static async Task<Results<Ok, NotFound<AuctionIdNotFound>, BadRequest<InvalidAuctionParameters>>>
        UpdateAuctionAsync(User user, AuctionUpdateRequest r, ICurrentTime currentTime, IRepository<Auction> repository)
    {
        var auctions = await repository.ListAsync();
        var auction = auctions.First(a => a.Id == r.Id);
        auction.EndTime = r.NewEnd;
        
        await repository.UpdateAsync(auction);
        await repository.CommitAsync();
        return TypedResults.Ok();
    }

    public static async Task<Ok<IEnumerable<AuctionId>>> ListAsync(User user, [FromRoute] AuctionStatus status,
        IAuctionEvaluator evaluator, IRepository<Auction> repository)
    {
        var auctions = await repository.ListAsync();
        var specified = auctions.Where(a => evaluator.EvaluateStatus(a) == status && a.Seller == user).Select(a => a.Id);
        return TypedResults.Ok(specified);
    }
    public static Task<Results<Ok<IEnumerable<SellerAuctionInfo>>, NotFound<AuctionIdNotFound>>> GetInfoAsync(User user, IEnumerable<AuctionId> auctionIds, IRepository<Auction> repository) => throw new NotImplementedException();
}