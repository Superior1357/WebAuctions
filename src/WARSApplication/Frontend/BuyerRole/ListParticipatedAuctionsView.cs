using Frontend.GenericInterfaces;
using Frontend.Utils;
using Backend.Data.APIData.Outgoing;

namespace Frontend.BuyerRole;

/// <summary>
/// View for listing all auctions the user participated in.
/// </summary>
public class ListParticipatedAuctionsViewController(IBuyMethodsApi api) : IViewController
{
    public string ViewName => "List participated auctions";
    
    /// <summary>
    /// List all auctions the user participated in.
    /// </summary>
    public async Task RunAsync()
    {
        var auctions = await api.ListParticipatedAuctionsAsync();
        var infos = (await api.ListAuctionsInfoAsync(auctions)).ToList();

        var scrollingList = new ScrollingListViewController<BuyerAuctionInfo>(
            "Auctions you participated in",
            infos,
            BuyerRole.Utils.BuyerAuctionInfoToListItem);

        _ = await scrollingList.MakeChoiceAsync();
    }
}