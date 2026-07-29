using System.Security.Claims;
using WARSApplication.APIData;
using WARSApplication.DataModel;
using WARSApplication.Repository;

namespace WARSApplication;

public static class AuctionManagement
{
    public static async Task<IResult> CreateAuctionAsync(AuctionAPI auctionApi, IAuctionRepository repository)
    {
        var auction = new Auction
        {
            // TODO: add sth here
        };
        
        await repository.AddAsync(auction);
        await repository.CommitAsync();
        
        return Results.Ok();
    }
    public static IResult UpdateAuction(UpdateRequest r, IAuctionRepository repository) => throw new NotImplementedException();

    public static async Task<IResult> ListAuctionsAsync(ClaimsPrincipal claim, IAuctionRepository repository)
    {
        var userId = claim.FindFirstValue(ClaimTypes.NameIdentifier);
        var g = claim.FindFirstValue(ClaimTypes.GivenName);
        var s = claim.FindFirstValue(ClaimTypes.Surname);
        
        Console.WriteLine($"user id: {userId}, given name: {g}, given surname: {s}");
        
        var auctions = await repository.AuctionsWhereAsync(a => a.Owner == userId).ToListAsync();
        return Results.Ok(auctions);
    }
    public static IResult AuctionInfo(AuctionId id, IAuctionRepository repository) => throw new NotImplementedException();
}

public record struct UpdateRequest(AuctionId Id, DateTime NewEnd); // do I need to have it like this?

public record struct AuctionInfo();