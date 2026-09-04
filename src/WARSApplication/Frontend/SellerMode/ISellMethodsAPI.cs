using Refit;
using WARSApplication.Data.APIData.Incoming;

namespace Frontend.SellerMode;

public interface ISellMethodsAPI
{
    [Post("/create")]
    public Task CreateAuctionAsync(CreatedAuction auction);

    [Post("/update")]
    public Task UpdateAuctionAsync(AuctionUpdateRequest updateRequest);
}