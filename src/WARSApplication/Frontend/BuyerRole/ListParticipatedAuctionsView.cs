using WARSApplication.Data.APIData.Outgoing;

namespace Frontend.BuyerMode;

public class ListParticipatedAuctionsView(IBuyMethodsAPI api) : IView
{
    public string Title => "List participated auctions";
    public async Task<IViewResult> RunAsync()
    {
        var auctions = await api.ListParticipatedAuctionsAsync();
        var infos = (await api.ListAuctionsInfoAsync(auctions)).ToList();

        var scrollingList = new ScrollingListView<BuyerAuctionInfo>(
            "Auctions you participated in",
            infos,
            Utils.BuyerAuctionInfoToListItem);

        _ = await scrollingList.MakeChoiceAsync();
        return new Result();
    }
}