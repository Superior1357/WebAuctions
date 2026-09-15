using WARSApplication.Data.APIData.Incoming;
using WARSApplication.Data.APIData.Outgoing;

namespace Frontend.SellerMode;

public class UpdateMode(SellerAuctionInfo auctionInfo, ISellMethodsAPI api) : IView
{
    public string Title => "Update auction";

    public async Task<IViewResult> RunAsync()
    {
        var newEnd = await Utils.PromptTimeAsync("New auction end: ");

        var updateRequest = new AuctionUpdateRequest(auctionInfo.Id, newEnd);
        
        await api.UpdateAuctionAsync(updateRequest);
        return new Result();
    }
}