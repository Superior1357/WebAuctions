using WARSApplication.Data.APIData.Outgoing;

namespace Frontend.BuyerMode;

public class ListWonAuctionsView(IBuyMethodsAPI api) : IView
{
    public string Title => "List won auctions";
    public async Task<IViewResult> RunAsync()
    {
        var auctions = await api.ListWonAuctionsAsync();
        var infos = (await api.ListAuctionsInfoAsync(auctions)).ToList();

        var scrollingList = new ScrollingListView<BuyerAuctionInfo>(
            "Auctions you won",
            infos,
            Utils.BuyerAuctionInfoToListItem);

        _ = await scrollingList.MakeChoiceAsync();
        return new Result();
    }
}