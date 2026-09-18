using Frontend.GenericInterfaces;
using Backend.Data.APIData.Incoming;
using Backend.Data.APIData.Outgoing;
namespace Frontend.SellerRole;

/// <summary>
/// View for updating an auction.
/// </summary>
public class UpdateMode(SellerAuctionInfo auctionInfo, ISellMethodsApi api) : IViewController
{
    public string ViewName => "Update auction";
    
    /// <summary>
    /// Ask the user for a new end time and send it to the API.
    /// </summary>
    public async Task RunAsync()
    {
        var newEnd = await Utils.Utils.PromptTimeAsync("New auction end: ");

        var updateRequest = new AuctionUpdateRequest(auctionInfo.Id, newEnd);
        
        await api.UpdateAuctionAsync(updateRequest);
    }
}