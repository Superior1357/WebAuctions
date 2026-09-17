using Frontend.GenericInterfaces;
using Frontend.Utils;
using Backend.Data.APIData.Outgoing;

namespace Frontend.BuyerRole;

/// <summary>
/// View for listing all auctions the user won.
/// </summary>
public class ListWonAuctionsViewController(IBuyMethodsApi api) : IViewController
{
    public string ViewName => "List won auctions";
    
    /// <summary>
    /// List all auctions the user won.
    /// </summary>
    public async Task RunAsync()
    {
        var auctions = await api.ListWonAuctionsAsync();
        var infos = (await api.ListAuctionsInfoAsync(auctions)).ToList();

        var scrollingList = new ScrollingListViewController<BuyerAuctionInfo>(
            "Auctions you won",
            infos,
            Utils.BuyerAuctionInfoToListItem);

        _ = await scrollingList.MakeChoiceAsync();
    }
}