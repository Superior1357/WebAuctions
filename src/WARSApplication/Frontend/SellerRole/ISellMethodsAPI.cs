using Refit;
using WARSApplication;
using WARSApplication.Data.APIData.Incoming;
using WARSApplication.Data.APIData.Outgoing;
using WARSApplication.Data.SharedDataProperties;

namespace Frontend.SellerRole;

public interface ISellMethodsApi
{
    [Post("/create")]
    public Task CreateAuctionAsync(CreatedAuction auction);

    [Patch("/update")]
    public Task UpdateAuctionAsync(AuctionUpdateRequest updateRequest);

    [Get("/list/{status}")]
    public Task<IEnumerable<AuctionId>> ListAuctionsAsync(AuctionStatus status);
    
    [Post("/info")]
    public Task<IEnumerable<SellerAuctionInfo>> GetInfoAsync(IEnumerable<AuctionId> auctionIds);
}