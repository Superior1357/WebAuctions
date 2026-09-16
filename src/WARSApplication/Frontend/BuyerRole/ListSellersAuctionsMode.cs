using Frontend.GenericInterfaces;
using Frontend.Utils;
using WARSApplication.Data.APIData.Outgoing;
using WARSApplication.Data.SharedDataProperties;

namespace Frontend.BuyerRole;

/// <summary>
/// List all auctions of a seller. 
/// </summary>
public class ListSellersAuctionsViewController(UserId userId, IBuyMethodsApi api, IMasterController masterController) : IViewController
{
    private int currentIndex = 0;
    public string ViewName => "List seller's auctions";
    
    /// <summary>
    /// List all auctions of a seller.
    /// Ask the user to choose one and show the bid view.
    /// </summary>
    public async Task RunAsync()
    {
        var auctions = await api.ListNotFinishedAuctionsOfAUserAsync(userId);
        var infos = (await api.ListAuctionsInfoAsync(auctions)).ToList();

        var scrollingListView = new ScrollingListViewController<BuyerAuctionInfo>("Choose an auction to bid", infos, BuyerRole.Utils.BuyerAuctionInfoToListItem);
        var choice = await scrollingListView.MakeChoiceAsync();

        if (choice == null)
            return;
        
        var view = new BidViewController(choice, api);
        await masterController.ShowViewAsync(view);
    }
}