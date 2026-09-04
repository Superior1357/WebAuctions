using Refit;
using WARSApplication.Data.SharedDataProperties;

namespace Frontend.BuyerMode;
using WARSApplication.Data.APIData.Outgoing;
using WARSApplication.Data.APIData.Incoming;


public interface IBuyMethodsAPI
{
    [Get("/list/sellers")]
    public Task<IEnumerable<UserId>> ListAllSellersAsync();
    
    [Post("/list/not_finished")]
    public Task<IEnumerable<AuctionId>> ListNotFinishedAuctionsOfAUserAsync([Body] UserId userId);

    [Post("/info")]
    public Task<IEnumerable<BuyerAuctionInfo>> ListAuctionsInfoAsync(IEnumerable<AuctionId> auctionIds);
    
    [Post("/bid")]
    public Task<string> BidAsync([Body] BidRequest bidRequest);
}