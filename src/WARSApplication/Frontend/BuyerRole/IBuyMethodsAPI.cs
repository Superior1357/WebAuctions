using Refit;
using Backend.Data.APIData.Incoming;
using Backend.Data.APIData.Outgoing;
using Backend.Data.SharedDataProperties;

namespace Frontend.BuyerRole;

public interface IBuyMethodsApi
{
    [Get("/list/sellers")]
    public Task<IEnumerable<UserId>> ListAllSellersAsync();
    
    [Post("/list/not_finished")]
    public Task<IEnumerable<AuctionId>> ListNotFinishedAuctionsOfAUserAsync([Body] UserId userId);
    
    [Get("/list/participated")]
    public Task<IEnumerable<AuctionId>> ListParticipatedAuctionsAsync();
    
    [Get("/list/won")]
    public Task<IEnumerable<AuctionId>> ListWonAuctionsAsync();
    
    [Post("/info")]
    public Task<IEnumerable<BuyerAuctionInfo>> ListAuctionsInfoAsync(IEnumerable<AuctionId> auctionIds);
    
    [Post("/bid")]
    public Task<string> BidAsync([Body] BidRequest bidRequest);
    
    
}